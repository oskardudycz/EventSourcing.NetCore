using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.BackgroundWorkers;

public class BackgroundWorker: BackgroundService
{
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

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(async () =>
        {
            await Task.Yield();
            logger.LogInformation("Background worker started");
            await perform(stoppingToken);
            logger.LogInformation("Background worker stopped");
        }, stoppingToken);
}
