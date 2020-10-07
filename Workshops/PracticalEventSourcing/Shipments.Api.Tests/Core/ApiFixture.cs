using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EventSourcing.Sample.IntegrationTests.Infrastructure;
using Xunit;

namespace Core.Testing
{
    public abstract class ApiFixture<TStartup>: IAsyncLifetime where TStartup : class
    {
        public readonly TestContext<TStartup> Sut;

        private HttpClient Client => Sut.Client;

        protected abstract string ApiUrl { get; }

        protected ApiFixture()
        {
            Sut = new TestContext<TStartup>();
        }

        public virtual Task InitializeAsync() => Task.CompletedTask;

        public virtual Task DisposeAsync() => Task.CompletedTask;

        public Task<HttpResponseMessage> GetAsync(string path = "")
        {
            return Client.GetAsync(
                $"{ApiUrl}/{path}"
            );
        }

        protected Task<HttpResponseMessage> PostAsync(object request)
        {
            return Client.PostAsync(
                ApiUrl,
                request.ToJsonStringContent()
            );
        }

        public IReadOnlyCollection<TEvent> PublishedEventsOfType<TEvent>()
            => Sut.PublishedEventsOfType<TEvent>();
    }
}
