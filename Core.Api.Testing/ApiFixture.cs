using System.Net;
using System.Net.Http.Headers;
using Core.Serialization.Newtonsoft;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Core.Api.Testing;

public abstract class ApiFixture<TStartup>: ApiFixture where TStartup : class
{
    public override TestContext CreateTestContext() =>
        new TestContext<TStartup>(GetConfiguration, SetupServices, SetupWebHostBuilder);
}

public abstract class ApiFixture: IAsyncLifetime
{
    protected readonly TestContext Sut;

    protected HttpClient Client => Sut.Client;

    protected TestServer Server => Sut.Server;

    protected abstract string ApiUrl { get; }

    protected virtual Dictionary<string, string> GetConfiguration(string fixtureName) => new();

    protected virtual Action<IServiceCollection>? SetupServices => null;

    protected virtual Func<IWebHostBuilder, IWebHostBuilder>? SetupWebHostBuilder => null;

    protected ApiFixture()
    {
        Environment.SetEnvironmentVariable("SchemaName", GetType().Name.ToLower());

        Sut = CreateTestContext();
    }

    public virtual TestContext CreateTestContext() => new(GetConfiguration, SetupServices, SetupWebHostBuilder);

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual Task DisposeAsync()
    {
        Sut.Dispose();
        Client.Dispose();
        return Task.CompletedTask;
    }

    // TODO: Add Poly here
    public async Task<HttpResponseMessage> Get(string path = "", int maxNumberOfRetries = 0,
        int retryIntervalInMs = 1000, Func<HttpResponseMessage, ValueTask<bool>>? check = null)
    {
        HttpResponseMessage queryResponse;
        var retryCount = maxNumberOfRetries;

        var doCheck = check ?? (response => new(response.StatusCode == HttpStatusCode.OK));
        do
        {
            queryResponse = await Client.GetAsync(
                $"{ApiUrl}/{path}"
            );

            if (retryCount == 0 || (await doCheck(queryResponse)))
                break;

            await Task.Delay(retryIntervalInMs);
            retryCount--;
        } while (true);

        return queryResponse;
    }

    public Task<HttpResponseMessage> Post(string path, object body,  RequestOptions? requestOptions = null)
        => Send(HttpMethod.Post, path, body, requestOptions);

    public Task<HttpResponseMessage> Post(object body,  RequestOptions? requestOptions = null) =>
        Post(string.Empty, body, requestOptions);

    public Task<HttpResponseMessage> Put(string path, object? body = null,  RequestOptions? requestOptions = null)
        => Send(HttpMethod.Put, path, body, requestOptions);

    public Task<HttpResponseMessage> Delete(string path, RequestOptions? requestOptions = null)
        => Send(HttpMethod.Delete, path, null, requestOptions);

    public Task<HttpResponseMessage> Delete(RequestOptions? requestOptions = null) =>
        Delete(string.Empty, requestOptions);

    public async Task<HttpResponseMessage> Send(HttpMethod method, string path, object? body = null, RequestOptions? requestOptions = null)
    {
        using var request = new HttpRequestMessage(method, $"{ApiUrl}/{path}");

        if (requestOptions?.IfMatch != null)
            request.Headers.IfMatch.Add(new EntityTagHeaderValue($"\"{requestOptions.IfMatch}\"", true));

        if (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch)
        {
            request.Content = body != null
                ? body.ToJsonStringContent()
                : new StringContent(string.Empty);
        }

        return await Client.SendAsync(request);
    }
}

public class RequestOptions
{
    public string? IfMatch { get; set; }
}
