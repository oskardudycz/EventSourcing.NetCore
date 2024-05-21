using Core.BackgroundWorkers;
using Core.Configuration;
using Core.Events;
using Core.EventStoreDB.Subscriptions;
using Core.EventStoreDB.Subscriptions.Batch;
using Core.EventStoreDB.Subscriptions.Checkpoints;
using Core.OpenTelemetry;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.EventStoreDB;

public class EventStoreDBConfig
{
    public string ConnectionString { get; set; } = default!;
}

public record EventStoreDBOptions(
    bool UseInternalCheckpointing = true
);

public static class EventStoreDBConfigExtensions
{
    private const string DefaultConfigKey = "EventStore";

    public static IServiceCollection AddEventStoreDB(
        this IServiceCollection services,
        IConfiguration config,
        EventStoreDBOptions? options = null
    ) =>
        services.AddEventStoreDB(
            config.GetRequiredConfig<EventStoreDBConfig>(DefaultConfigKey),
            options
        );

    public static IServiceCollection AddEventStoreDB(
        this IServiceCollection services,
        EventStoreDBConfig eventStoreDBConfig,
        EventStoreDBOptions? options = null
    )
    {
        services
            .AddSingleton(EventTypeMapper.Instance)
            .AddSingleton(new EventStoreClient(EventStoreClientSettings.Create(eventStoreDBConfig.ConnectionString)));

        if (options?.UseInternalCheckpointing != false)
        {
            services
                .AddTransient<ISubscriptionCheckpointRepository, EventStoreDBSubscriptionCheckpointRepository>();
        }

        return services.AddHostedService(serviceProvider =>
            {
                var logger =
                    serviceProvider.GetRequiredService<ILogger<BackgroundWorker>>();

                var coordinator = serviceProvider.GetRequiredService<EventStoreDBSubscriptioToAllCoordinator>();

                TelemetryPropagator.UseDefaultCompositeTextMapPropagator();

                return new BackgroundWorker<EventStoreDBSubscriptioToAllCoordinator>(
                    coordinator,
                    logger,
                    (c, ct) => c.SubscribeToAll(ct)
                );
            }
        );
    }

    public static IServiceCollection AddEventStoreDBSubscriptionToAll(
        this IServiceCollection services,
        EventStoreDBSubscriptionToAllOptions subscriptionOptions,
        bool checkpointToEventStoreDB = true)
    {
        services.AddScoped<EventsBatchProcessor, EventsBatchProcessor>();
        services.AddScoped<IEventsBatchCheckpointer, EventsBatchCheckpointer>();
        services.AddSingleton<EventStoreDBSubscriptioToAllCoordinator>();

        if (checkpointToEventStoreDB)
        {
            services
                .AddSingleton<ISubscriptionCheckpointRepository, EventStoreDBSubscriptionCheckpointRepository>();
        }

        return services.AddKeyedSingleton<EventStoreDBSubscriptionToAll>(
            $"ESDB_subscription-{subscriptionOptions.SubscriptionId}",
            (sp, _) => new EventStoreDBSubscriptionToAll(
                subscriptionOptions,
                sp.GetRequiredService<EventStoreClient>(),
                sp,
                sp.GetRequiredService<ILogger<EventStoreDBSubscriptionToAll>>()
            )
        );
    }
}
