using Core.BackgroundWorkers;
using Core.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.EventStoreDB.Subscriptions.Checkpoints.Postgres;

public static class PostgresCheckpointingConfiguration
{
    public static IServiceCollection AddPostgresCheckpointing(this IServiceCollection services) =>
        services.AddScoped<PostgresSubscriptionCheckpointSetup>()
            .AddScoped<ISubscriptionCheckpointRepository, PostgresSubscriptionCheckpointRepository>()
            .AddHostedService(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<BackgroundWorker<
                    PostgresSubscriptionCheckpointSetup>>>();
                var checkpointSetup = serviceProvider.GetRequiredService<PostgresSubscriptionCheckpointSetup>();

                TelemetryPropagator.UseDefaultCompositeTextMapPropagator();

                return new BackgroundWorker<PostgresSubscriptionCheckpointSetup>(
                    checkpointSetup,
                    logger,
                    async (setup, ct) => await setup.EnsureCheckpointsTableExist(ct).ConfigureAwait(false)
                );
            }
        );
}
