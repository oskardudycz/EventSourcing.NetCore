using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;
using Core.Api.Testing;
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
    public static Func<HttpClient, HttpRequestMessage, Task<HttpResponseMessage>> GET = SEND(HttpMethod.Get);

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
    public static Func<HttpResponseMessage, ValueTask> OK = HTTP_STATUS(HttpStatusCode.OK);

    public static Func<HttpResponseMessage, ValueTask> CREATED =>
        async response =>
        {
            await HTTP_STATUS(HttpStatusCode.Created)(response);

            var locationHeader = response.Headers.Location;

            locationHeader.Should().NotBeNull();

            var location = locationHeader!.OriginalString;

            location.Should().StartWith(response.RequestMessage!.RequestUri!.AbsolutePath);
        };

    public static Func<HttpResponseMessage, ValueTask> BAD_REQUEST = HTTP_STATUS(HttpStatusCode.BadRequest);
    public static Func<HttpResponseMessage, ValueTask> NOT_FOUND = HTTP_STATUS(HttpStatusCode.NotFound);
    public static Func<HttpResponseMessage, ValueTask> CONFLICT = HTTP_STATUS(HttpStatusCode.Conflict);

    public static Func<HttpResponseMessage, ValueTask> PRECONDITION_FAILED =
        HTTP_STATUS(HttpStatusCode.PreconditionFailed);

    public static Func<HttpResponseMessage, ValueTask>
        METHOD_NOT_ALLOWED = HTTP_STATUS(HttpStatusCode.MethodNotAllowed);

    public static Func<HttpResponseMessage, ValueTask> HTTP_STATUS(HttpStatusCode status) =>
        response =>
        {
            response.StatusCode.Should().Be(status);
            return ValueTask.CompletedTask;
        };

    public static Func<HttpResponseMessage, ValueTask> RESPONSE_BODY<T>(T body) =>
        RESPONSE_BODY<T>(result => result.Should().BeEquivalentTo(body));

    public static Func<HttpResponseMessage, ValueTask> RESPONSE_BODY<T>(Action<T> assert) =>
        async response =>
        {
            var result = await response.GetResultFromJson<T>();
            assert(result);

            result.Should().BeEquivalentTo(result);
        };
}

public class ApiSpecification<TProgram>: IDisposable where TProgram : class
{
    private readonly WebApplicationFactory<TProgram> applicationFactory;
    private readonly HttpClient client;

    public ApiSpecification(): this(new WebApplicationFactory<TProgram>())
    {
    }

    protected ApiSpecification(WebApplicationFactory<TProgram> applicationFactory)
    {
        this.applicationFactory = applicationFactory;
        client = applicationFactory.CreateClient();
    }

    public static ApiSpecification<TProgram> Setup(WebApplicationFactory<TProgram> applicationFactory) =>
        new(applicationFactory);

    public async Task<HttpResponseMessage> Send(ApiRequest apiRequest) =>
        (await Send(new[] { apiRequest })).Single();

    public async Task<HttpResponseMessage[]> Send(params ApiRequest[] apiRequests)
    {
        var responses = new List<HttpResponseMessage>();

        foreach (var request in apiRequests)
        {
            responses.Add(await client.Send(request));
        }

        return responses.ToArray();
    }

    public GivenApiSpecificationBuilder Given(
        params Func<HttpRequestMessage, HttpRequestMessage>[] builders) =>
        new(client, builders);

    /////////////////////
    ////   BUILDER   ////
    /////////////////////

    public class GivenApiSpecificationBuilder
    {
        private readonly Func<HttpRequestMessage, HttpRequestMessage>[] given;
        private readonly HttpClient client;

        internal GivenApiSpecificationBuilder(HttpClient client, Func<HttpRequestMessage, HttpRequestMessage>[] given)
        {
            this.client = client;
            this.given = given;
        }

        public WhenApiSpecificationBuilder When(Func<HttpClient, HttpRequestMessage, Task<HttpResponseMessage>> when) =>
            new(client, given, when);
    }

    public class WhenApiSpecificationBuilder
    {
        private readonly Func<HttpRequestMessage, HttpRequestMessage>[] given;
        private readonly Func<HttpClient, HttpRequestMessage, Task<HttpResponseMessage>> when;
        private readonly HttpClient client;

        internal WhenApiSpecificationBuilder(
            HttpClient client,
            Func<HttpRequestMessage, HttpRequestMessage>[] given,
            Func<HttpClient, HttpRequestMessage, Task<HttpResponseMessage>> when
        )
        {
            this.client = client;
            this.given = given;
            this.when = when;
        }

        public Task<ThenApiSpecificationBuilder> Then(Func<HttpResponseMessage, ValueTask> then) =>
            Then(new[] { then });

        public async Task<ThenApiSpecificationBuilder> Then(params Func<HttpResponseMessage, ValueTask>[] thens)
        {
            var request = new ApiRequest(when, given);

            var response = await client.Send(request);

            foreach (var then in thens)
            {
                await then(response);
            }

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
        client.Dispose();
        applicationFactory.Dispose();
    }
}

public class ApiRequest
{
    private readonly Func<HttpClient, HttpRequestMessage, Task<HttpResponseMessage>> send;
    private readonly Func<HttpRequestMessage, HttpRequestMessage>[] builders;

    public ApiRequest(
        Func<HttpClient, HttpRequestMessage, Task<HttpResponseMessage>> send,
        params Func<HttpRequestMessage, HttpRequestMessage>[] builders
    )
    {
        this.send = send;
        this.builders = builders;
    }

    public Task<HttpResponseMessage> Send(HttpClient httpClient)
    {
        var request = builders.Aggregate(
            new HttpRequestMessage(),
            (request, build) => build(request)
        );

        return send(httpClient, request);
    }
}

public static class ApiRequestExtensions
{
    public static Task<HttpResponseMessage> Send(this HttpClient httpClient, ApiRequest apiRequest) =>
        apiRequest.Send(httpClient);
}

public static class HttpResponseMessageExtensions
{
    public static bool TryGetCreatedId<T>(this HttpResponseMessage response, out T? value)
    {
        value = default;
        var requestAbsolutePath = response.RequestMessage?.RequestUri?.AbsolutePath;

        if (string.IsNullOrEmpty(requestAbsolutePath))
            return false;

        var createdId =
            response.Headers.Location?.OriginalString.Replace(requestAbsolutePath, "");

        if (createdId == null)
            return false;

        var result = TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(createdId);

        if (result == null)
            return false;

        value = (T?)result;

        return true;
    }

    public static T GetCreatedId<T>(this HttpResponseMessage response) =>
        response.TryGetCreatedId<T>(out var createdId)
            ? createdId!
            : throw new ArgumentOutOfRangeException(nameof(response.Headers.Location));
}
