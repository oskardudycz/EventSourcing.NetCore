using System.Threading;
using System.Threading.Tasks;
using Core.Events.External;

namespace Meetings.IntegrationTests.Infrastructure
{
    public class DummyExternalEventConsumer: IExternalEventConsumer
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
