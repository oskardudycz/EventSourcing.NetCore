using Microsoft.Extensions.DependencyInjection;

namespace Core.EventStoreDB.Subscriptions.Checkpoints.Postgres;

public static class PostgresCheckpointingConfiguration
{
    public static IServiceCollection AddPostgresCheckpointing(this IServiceCollection services) =>
        services
            .AddScoped<ISubscriptionCheckpointRepository, PostgresSubscriptionCheckpointRepository>()
            .AddSingleton<ISubscriptionStoreSetup, PostgresSubscriptionCheckpointSetup>();
}
