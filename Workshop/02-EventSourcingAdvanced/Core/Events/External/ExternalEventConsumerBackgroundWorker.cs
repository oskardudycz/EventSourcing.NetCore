using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.Events.External
{
    public class ExternalEventConsumerBackgroundWorker: IHostedService
    {
        private readonly IExternalEventConsumer externalEventConsumer;
        private readonly ILogger<ExternalEventConsumerBackgroundWorker> logger;

        public ExternalEventConsumerBackgroundWorker(
            IExternalEventConsumer externalEventConsumer,
            ILogger<ExternalEventConsumerBackgroundWorker> logger
        )
        {
            this.externalEventConsumer = externalEventConsumer ?? throw new ArgumentNullException(nameof(externalEventConsumer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("External Event Consumer started");

            return externalEventConsumer.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("External Event Consumer stoped");

            return Task.CompletedTask;
        }
    }
}
