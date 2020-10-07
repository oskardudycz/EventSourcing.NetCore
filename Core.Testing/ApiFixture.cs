using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Events;
using EventSourcing.Sample.IntegrationTests.Infrastructure;
using Xunit;

namespace Core.Testing
{
    public abstract class ApiFixture<TStartup>: IAsyncLifetime where TStartup : class
    {
        protected readonly TestContext<TStartup> Sut;

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

        protected Task<HttpResponseMessage> PostAsync(string path, object request)
        {
            return Client.PostAsync(
                $"{ApiUrl}/{path}",
                request.ToJsonStringContent()
            );
        }

        protected Task<HttpResponseMessage> PostAsync(object request)
        {
            return PostAsync(string.Empty, request);
        }

        public IReadOnlyCollection<TEvent> PublishedExternalEventsOfType<TEvent>() where TEvent: IExternalEvent
            => Sut.PublishedExternalEventsOfType<TEvent>();

        public IReadOnlyCollection<TEvent> PublishedInternalEventsOfType<TEvent>() where TEvent: IEvent
            => Sut.PublishedInternalEventsOfType<TEvent>();
    }
}
