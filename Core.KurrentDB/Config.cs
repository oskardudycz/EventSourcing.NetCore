using Core.BackgroundWorkers;
using Core.Configuration;
using Core.Events;
using Core.KurrentDB.Subscriptions;
using Core.KurrentDB.Subscriptions.Batch;
using Core.KurrentDB.Subscriptions.Checkpoints;
using Core.OpenTelemetry;
using KurrentDB.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.KurrentDB;

public class KurrentDBConfig
{
    public string ConnectionString { get; set; } = null!;
}

public record KurrentDBOptions(
    bool UseInternalCheckpointing = true
);

public static class KurrentDBConfigExtensions
{
    private const string DefaultConfigKey = "KurrentDB";

    public static IServiceCollection AddKurrentDB(
        this IServiceCollection services,
        IConfiguration config,
        KurrentDBOptions? options = null
    ) =>
        services.AddKurrentDB(
            config.GetRequiredConfig<KurrentDBConfig>(DefaultConfigKey),
            options
        );

    public static IServiceCollection AddKurrentDB(
        this IServiceCollection services,
        KurrentDBConfig kurrentDBConfig,
        KurrentDBOptions? options = null
    )
    {
        services
            .AddSingleton(EventTypeMapper.Instance)
            .AddSingleton(new KurrentDBClient(KurrentDBClientSettings.Create(kurrentDBConfig.ConnectionString)))
            .AddScoped<EventsBatchProcessor, EventsBatchProcessor>()
            .AddScoped<IEventsBatchCheckpointer, EventsBatchCheckpointer>()
            .AddSingleton<ISubscriptionStoreSetup, NulloSubscriptionStoreSetup>();

        if (options?.UseInternalCheckpointing != false)
        {
            services
                .AddTransient<ISubscriptionCheckpointRepository, KurrentDBSubscriptionCheckpointRepository>();
        }

        return services.AddHostedService(serviceProvider =>
            {
                var logger =
                    serviceProvider.GetRequiredService<ILogger<BackgroundWorker>>();

                var coordinator = serviceProvider.GetRequiredService<KurrentDBSubscriptionsToAllCoordinator>();

                TelemetryPropagator.UseDefaultCompositeTextMapPropagator();

                return new BackgroundWorker<KurrentDBSubscriptionsToAllCoordinator>(
                    coordinator,
                    logger,
                    (c, ct) => c.SubscribeToAll(ct)
                );
            }
        );
    }


    public static IServiceCollection AddKurrentDBSubscriptionToAll<THandler>(
        this IServiceCollection services,
        string subscriptionId
    ) where THandler : IEventBatchHandler =>
        services.AddKurrentDBSubscriptionToAll(
            new KurrentDBSubscriptionToAllOptions { SubscriptionId = subscriptionId },
            sp => [sp.GetRequiredService<THandler>()]
        );


    public static IServiceCollection AddKurrentDBSubscriptionToAll<THandler>(
        this IServiceCollection services,
        KurrentDBSubscriptionToAllOptions subscriptionOptions
    ) where THandler : IEventBatchHandler =>
        services.AddKurrentDBSubscriptionToAll(subscriptionOptions, sp => [sp.GetRequiredService<THandler>()]);

    public static IServiceCollection AddKurrentDBSubscriptionToAll(
        this IServiceCollection services,
        KurrentDBSubscriptionToAllOptions subscriptionOptions,
        Func<IServiceProvider, IEventBatchHandler[]> handlers
    )
    {
        services.AddSingleton<KurrentDBSubscriptionsToAllCoordinator>();

        return services.AddKeyedSingleton<KurrentDBSubscriptionToAll>(
            subscriptionOptions.SubscriptionId,
            (sp, _) =>
            {
                var subscription = new KurrentDBSubscriptionToAll(
                    sp.GetRequiredService<KurrentDBClient>(),
                    sp.GetRequiredService<IServiceScopeFactory>(),
                    sp.GetRequiredService<ILogger<KurrentDBSubscriptionToAll>>()
                ) { Options = subscriptionOptions, GetHandlers = handlers };

                return subscription;
            });
    }
}
