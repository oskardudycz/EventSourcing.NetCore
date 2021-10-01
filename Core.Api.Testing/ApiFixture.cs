using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Core.Api.Testing
{
    public abstract class ApiFixture<TStartup>: ApiFixture where TStartup : class
    {
        public override TestContext CreateTestContext(ITestOutputHelper testOutputHelper) =>
            new TestContext<TStartup>(GetConfiguration, SetupServices, SetupWebHostBuilder, testOutputHelper);
    }

    public abstract class ApiFixture: IAsyncDisposable
    {
        protected TestContext Sut = default!;

        protected HttpClient Client => Sut.Client;

        protected TestServer Server => Sut.Server;

        protected abstract string ApiUrl { get; }

        protected virtual Dictionary<string, string> GetConfiguration(string fixtureName) => new();

        protected virtual Action<IServiceCollection>? SetupServices => null;

        protected virtual Func<IWebHostBuilder, IWebHostBuilder>? SetupWebHostBuilder => null;


        protected ApiFixture()
        {
            Environment.SetEnvironmentVariable("SchemaName", GetType().Name.ToLower());
        }

        public virtual TestContext CreateTestContext(ITestOutputHelper testOutputHelper) =>
            new(
                GetConfiguration,
                SetupServices,
                SetupWebHostBuilder,
                testOutputHelper
            );

        public Task InitializeAsync(ITestOutputHelper testOutputHelper)
        {
            Sut = CreateTestContext(testOutputHelper);
            return Setup();
        }

        protected abstract Task Setup();

        public virtual ValueTask DisposeAsync() => new ();

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

        public Task<HttpResponseMessage> Post(string path, object request)
        {
            return Client.PostAsync(
                $"{ApiUrl}/{path}",
                request.ToJsonStringContent()
            );
        }

        public Task<HttpResponseMessage> Post(object request)
        {
            return Post(string.Empty, request);
        }

        public Task<HttpResponseMessage> Put(string path, object? request = null)
        {
            return Client.PutAsync(
                $"{ApiUrl}/{path}",
                request != null
                    ? request.ToJsonStringContent()
                    : new StringContent(string.Empty)
            );
        }

        public Task<HttpResponseMessage> Put(object request)
        {
            return Put(string.Empty, request);
        }
    }
}
