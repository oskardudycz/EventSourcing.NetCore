using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.BackgroundWorkers;

public class BackgroundWorker(
    ILogger<BackgroundWorker> logger,
    Func<CancellationToken, Task> perform)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(async () =>
        {
            await Task.Yield();
            logger.LogInformation("Background worker started");
            await perform(stoppingToken).ConfigureAwait(false);
            logger.LogInformation("Background worker stopped");
        }, stoppingToken);
}
