using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Events;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Core.Testing
{
    public abstract class ApiFixture<TStartup>: ApiFixture where TStartup : class
    {
        public override TestContext CreateTestContext() =>
            new TestContext<TStartup>(GetConfiguration, SetupServices, SetupWebHostBuilder);
    }

    public abstract class ApiFixture: IAsyncLifetime
    {
        protected readonly TestContext Sut;

        private HttpClient Client => Sut.Client;

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

        public virtual Task DisposeAsync() => Task.CompletedTask;

        public Task<HttpResponseMessage> Get(string path = "")
        {
            return Client.GetAsync(
                $"{ApiUrl}/{path}"
            );
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

        public IReadOnlyCollection<TEvent> PublishedExternalEventsOfType<TEvent>() where TEvent : IExternalEvent
            => Sut.PublishedExternalEventsOfType<TEvent>();

        public IReadOnlyCollection<TEvent> PublishedInternalEventsOfType<TEvent>() where TEvent : IEvent
            => Sut.PublishedInternalEventsOfType<TEvent>();
    }
}
