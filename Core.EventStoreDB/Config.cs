using Core.BackgroundWorkers;
using Core.Configuration;
using Core.Events;
using Core.EventStoreDB.Repository;
using Core.EventStoreDB.Subscriptions;
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
            .AddTransient<EventStoreDBSubscriptionToAll, EventStoreDBSubscriptionToAll>();

        if (options?.UseInternalCheckpointing != false)
        {
            services
                .AddTransient<ISubscriptionCheckpointRepository, EventStoreDBSubscriptionCheckpointRepository>();
        }

        return services;
    }

    public static IServiceCollection AddEventStoreDBSubscriptionToAll(
        this IServiceCollection services,
        EventStoreDBSubscriptionToAllOptions subscriptionOptions,
        bool checkpointToEventStoreDB = true)
    {
        if (checkpointToEventStoreDB)
        {
            services
                .AddTransient<ISubscriptionCheckpointRepository, EventStoreDBSubscriptionCheckpointRepository>();
        }

        return services.AddHostedService(serviceProvider =>
            {
                var logger =
                    serviceProvider.GetRequiredService<ILogger<BackgroundWorker>>();

                var eventStoreDBSubscriptionToAll =
                    serviceProvider.GetRequiredService<EventStoreDBSubscriptionToAll>();

                TelemetryPropagator.UseDefaultCompositeTextMapPropagator();

                return new BackgroundWorker(
                    logger,
                    ct => eventStoreDBSubscriptionToAll.SubscribeToAll(subscriptionOptions, ct)
                );
            }
        );
    }
}
