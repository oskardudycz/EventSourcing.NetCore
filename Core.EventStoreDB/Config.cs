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
            .AddSingleton(new EventStoreClient(EventStoreClientSettings.Create(eventStoreDBConfig.ConnectionString)))
            .AddScoped<EventsBatchProcessor, EventsBatchProcessor>()
            .AddScoped<IEventsBatchCheckpointer, EventsBatchCheckpointer>()
            .AddSingleton<ISubscriptionStoreSetup, NulloSubscriptionStoreSetup>();

        if (options?.UseInternalCheckpointing != false)
        {
            services
                .AddTransient<ISubscriptionCheckpointRepository, EventStoreDBSubscriptionCheckpointRepository>();
        }

        return services.AddHostedService(serviceProvider =>
            {
                var logger =
                    serviceProvider.GetRequiredService<ILogger<BackgroundWorker>>();

                var coordinator = serviceProvider.GetRequiredService<EventStoreDBSubscriptionsToAllCoordinator>();

                TelemetryPropagator.UseDefaultCompositeTextMapPropagator();

                return new BackgroundWorker<EventStoreDBSubscriptionsToAllCoordinator>(
                    coordinator,
                    logger,
                    (c, ct) => c.SubscribeToAll(ct)
                );
            }
        );
    }


    public static IServiceCollection AddEventStoreDBSubscriptionToAll<THandler>(
        this IServiceCollection services,
        string subscriptionId
    ) where THandler : IEventBatchHandler =>
        services.AddEventStoreDBSubscriptionToAll(
            new EventStoreDBSubscriptionToAllOptions { SubscriptionId = subscriptionId },
            sp => [sp.GetRequiredService<THandler>()]
        );


    public static IServiceCollection AddEventStoreDBSubscriptionToAll<THandler>(
        this IServiceCollection services,
        EventStoreDBSubscriptionToAllOptions subscriptionOptions
    ) where THandler : IEventBatchHandler =>
        services.AddEventStoreDBSubscriptionToAll(subscriptionOptions, sp => [sp.GetRequiredService<THandler>()]);

    public static IServiceCollection AddEventStoreDBSubscriptionToAll(
        this IServiceCollection services,
        EventStoreDBSubscriptionToAllOptions subscriptionOptions,
        Func<IServiceProvider, IEventBatchHandler[]> handlers
    )
    {
        services.AddSingleton<EventStoreDBSubscriptionsToAllCoordinator>();

        return services.AddKeyedSingleton<EventStoreDBSubscriptionToAll>(
            subscriptionOptions.SubscriptionId,
            (sp, _) =>
            {
                var subscription = new EventStoreDBSubscriptionToAll(
                    sp.GetRequiredService<EventStoreClient>(),
                    sp.GetRequiredService<IServiceScopeFactory>(),
                    sp.GetRequiredService<ILogger<EventStoreDBSubscriptionToAll>>()
                ) { Options = subscriptionOptions, GetHandlers = handlers };

                return subscription;
            });
    }
}
