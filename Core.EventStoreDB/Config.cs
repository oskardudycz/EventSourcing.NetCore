using System;
using Core.EventStoreDB.OptimisticConcurrency;
using Core.EventStoreDB.Subscriptions;
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

    public static IServiceCollection AddEventStoreDB(this IServiceCollection services, IConfiguration config, EventStoreDBOptions? options = null)
    {
        var eventStoreDBConfig = config.GetSection(DefaultConfigKey).Get<EventStoreDBConfig>();

        services.AddSingleton(
            new EventStoreClient(EventStoreClientSettings.Create(eventStoreDBConfig.ConnectionString)))
            .AddScoped<EventStoreDBExpectedStreamRevisionProvider, EventStoreDBExpectedStreamRevisionProvider>()
            .AddScoped<EventStoreDBNextStreamRevisionProvider, EventStoreDBNextStreamRevisionProvider>();

        if (options?.UseInternalCheckpointing != false)
        {
            services
                .AddTransient<ISubscriptionCheckpointRepository, EventStoreDBSubscriptionCheckpointRepository>();
        }

        return services;
    }

    public static IServiceCollection AddEventStoreDBSubscriptionToAll(
        this IServiceCollection services,
        string subscriptionId,
        SubscriptionFilterOptions? filterOptions = null,
        Action<EventStoreClientOperationOptions>? configureOperation = null,
        UserCredentials? credentials = null,
        bool checkpointToEventStoreDB = true)
    {
        if (checkpointToEventStoreDB)
        {
            services
                .AddTransient<ISubscriptionCheckpointRepository, EventStoreDBSubscriptionCheckpointRepository>();
        }

        return services.AddHostedService(serviceProvider =>
            new SubscribeToAllBackgroundWorker(
                serviceProvider,
                serviceProvider.GetRequiredService<EventStoreClient>(),
                serviceProvider.GetRequiredService<ISubscriptionCheckpointRepository>(),
                serviceProvider.GetRequiredService<ILogger<SubscribeToAllBackgroundWorker>>(),
                subscriptionId,
                filterOptions,
                configureOperation,
                credentials
            )
        );
    }
}
