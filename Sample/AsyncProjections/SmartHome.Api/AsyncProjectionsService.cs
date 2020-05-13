using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Marten;
using Marten.Events.Projections.Async;
using Microsoft.Extensions.Hosting;

namespace SmartHome.Api
{
    public class AsyncProjectionsService : BackgroundService
    {
        private readonly IDaemon daemon;

        public AsyncProjectionsService(IDocumentStore store)
        {
            daemon = store.BuildProjectionDaemon(settings: new DaemonSettings
            {
                LeadingEdgeBuffer = 0.Seconds()
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Start all of the configured async projections
            daemon.StartAll();

            // Runs all projections until there are no more events coming in
            await daemon.WaitForNonStaleResults(stoppingToken).ConfigureAwait(false);
        }
    }
}
