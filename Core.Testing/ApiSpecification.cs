using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Ogooreck.API;

public static class ApiSpecification
{
    ///////////////////
    ////   GIVEN   ////
    ///////////////////
    public static Func<HttpRequestMessage, HttpRequestMessage> URI(string uri) =>
        URI(new Uri(uri, UriKind.RelativeOrAbsolute));

    public static Func<HttpRequestMessage, HttpRequestMessage> URI(Uri uri) =>
        request =>
        {
            request.RequestUri = uri;
            return request;
        };

    public static Func<HttpRequestMessage, HttpRequestMessage> BODY<T>(T body) =>
        request =>
        {
            request.Content = JsonContent.Create(body);

            return request;
        };

    ///////////////////
    ////   WHEN    ////
    ///////////////////
    public static Func<HttpClient, HttpRequestMessage, Task<HttpResponseMessage>> GET = SEND(HttpMethod.Post);

    public static Func<HttpClient, HttpRequestMessage, Task<HttpResponseMessage>> POST = SEND(HttpMethod.Post);

    public static Func<HttpClient, HttpRequestMessage, Task<HttpResponseMessage>> PUT = SEND(HttpMethod.Put);

    public static Func<HttpClient, HttpRequestMessage, Task<HttpResponseMessage>> DELETE = SEND(HttpMethod.Delete);

    public static Func<HttpClient, HttpRequestMessage, Task<HttpResponseMessage>> SEND(HttpMethod httpMethod) =>
        (api, request) =>
        {
            request.Method = httpMethod;
            return api.SendAsync(request);
        };

    ///////////////////
    ////   THEN    ////
    ///////////////////
    public static Action<HttpResponseMessage> OK = HTTP_STATUS(HttpStatusCode.OK);

    public static Action<HttpResponseMessage> CREATED =>
        response =>
        {
            HTTP_STATUS(HttpStatusCode.Created);

            var locationHeader = response.Headers.Location;

            locationHeader.Should().NotBeNull();

            var location = locationHeader!.OriginalString;

            location.Should().StartWith(response.RequestMessage!.RequestUri!.AbsolutePath);
        };

    public static Action<HttpResponseMessage> BAD_REQUEST = HTTP_STATUS(HttpStatusCode.BadRequest);
    public static Action<HttpResponseMessage> NOT_FOUND = HTTP_STATUS(HttpStatusCode.NotFound);
    public static Action<HttpResponseMessage> CONFLICT = HTTP_STATUS(HttpStatusCode.Conflict);
    public static Action<HttpResponseMessage> PRECONDITION_FAILED = HTTP_STATUS(HttpStatusCode.PreconditionFailed);
    public static Action<HttpResponseMessage> METHOD_NOT_ALLOWED = HTTP_STATUS(HttpStatusCode.MethodNotAllowed);

    public static Action<HttpResponseMessage> HTTP_STATUS(HttpStatusCode status) =>
        response => response.StatusCode.Should().Be(status);
}

public class ApiSpecification<TProgram>: IDisposable where TProgram : class
{
    private readonly WebApplicationFactory<TProgram> applicationFactory;
    private readonly HttpClient client;

    public ApiSpecification(): this(new WebApplicationFactory<TProgram>())
    {
    }

    public ApiSpecification(WebApplicationFactory<TProgram> applicationFactory)
    {
        this.applicationFactory = applicationFactory;
        client = applicationFactory.CreateClient();
    }

    public GivenApiSpecificationBuilder<HttpRequestMessage> Given(
        params Func<HttpRequestMessage, HttpRequestMessage>[] builders)
    {
        var define = () => new HttpRequestMessage();

        foreach (var current in builders)
        {
            var previous = define;
            define = () => current(previous());
        }

        return new GivenApiSpecificationBuilder<HttpRequestMessage>(client, define);
    }

    /////////////////////
    ////   BUILDER   ////
    /////////////////////

    public class GivenApiSpecificationBuilder<TRequest>
    {
        private readonly Func<TRequest> given;
        private readonly HttpClient client;

        internal GivenApiSpecificationBuilder(HttpClient client, Func<TRequest> given)
        {
            this.client = client;
            this.given = given;
        }

        public WhenApiSpecificationBuilder<TRequest> When(Func<HttpClient, TRequest, Task<HttpResponseMessage>> when) =>
            new(client, given, when);
    }

    public class WhenApiSpecificationBuilder<TRequest>
    {
        private readonly Func<TRequest> given;
        private readonly Func<HttpClient, TRequest, Task<HttpResponseMessage>> when;
        private readonly HttpClient client;

        internal WhenApiSpecificationBuilder(HttpClient client, Func<TRequest> given,
            Func<HttpClient, TRequest, Task<HttpResponseMessage>> when)
        {
            this.client = client;
            this.given = given;
            this.when = when;
        }

        public async Task<ThenApiSpecificationBuilder> Then(Action<HttpResponseMessage> then)
        {
            var request = given();

            var response = await when(client, request);

            then(response);

            return new ThenApiSpecificationBuilder(response);
        }
    }

    public class ThenApiSpecificationBuilder
    {
        private readonly HttpResponseMessage response;

        internal ThenApiSpecificationBuilder(HttpResponseMessage response)
        {
            this.response = response;
        }

        public ThenApiSpecificationBuilder And(Action<HttpResponseMessage> then)
        {
            then(response);

            return this;
        }
    }

    public void Dispose()
    {
        applicationFactory.Dispose();
        client.Dispose();
    }
}
