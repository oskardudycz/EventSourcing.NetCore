using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Api.Core
{
    public class BackgroundWorker: IHostedService
    {
        private Task? executingTask;
        private CancellationTokenSource? cts;
        private readonly ILogger<BackgroundWorker> logger;
        private readonly Func<CancellationToken, Task> perform;

        public BackgroundWorker(
            ILogger<BackgroundWorker> logger,
            Func<CancellationToken, Task> perform
        )
        {
            this.logger = logger;
            this.perform = perform;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a linked token so we can trigger cancellation outside of this token's cancellation
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            executingTask = perform(cts.Token);

            return executingTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (executingTask == null)
                return;

            // Signal cancellation to the executing method
            cts?.Cancel();

            // Wait until the issue completes or the stop token triggers
            await Task.WhenAny(executingTask, Task.Delay(-1, cancellationToken));

            // Throw if cancellation triggered
            cancellationToken.ThrowIfCancellationRequested();

            logger.LogInformation("Background worker stopped");
        }
    }
}
