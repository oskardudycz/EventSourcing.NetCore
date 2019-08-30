using System.Net.Http;
using System.Threading.Tasks;
using EventSourcing.Sample.IntegrationTests.Infrastructure;
using Xunit;

namespace Meetings.IntegrationTests.Infrastructure
{
    public abstract class ApiFixture<TStartup>: IAsyncLifetime where TStartup : class
    {
        public readonly TestContext<TStartup> Sut;

        public HttpClient Client => Sut.Client;

        public abstract string ApiUrl { get; }

        protected ApiFixture()
        {
            Sut = new TestContext<TStartup>();
        }

        public virtual Task InitializeAsync() => Task.CompletedTask;

        public virtual Task DisposeAsync() => Task.CompletedTask;
    }
}
