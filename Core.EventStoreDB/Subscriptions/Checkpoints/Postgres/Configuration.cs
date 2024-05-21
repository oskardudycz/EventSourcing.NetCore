using Core.BackgroundWorkers;
using Core.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Core.EventStoreDB.Subscriptions.Checkpoints.Postgres;

public static class PostgresCheckpointingConfiguration
{
    public static IServiceCollection AddPostgresCheckpointing(this IServiceCollection services) =>
        services
            .AddScoped<ISubscriptionCheckpointRepository, PostgresSubscriptionCheckpointRepository>()
            .AddSingleton<ISubscriptionStoreSetup, PostgresSubscriptionCheckpointSetup>();
}
