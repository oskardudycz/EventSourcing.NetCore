using ECommerce.Core.Subscriptions;
using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core.EventStoreDB;

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
            .AddTransient<EventStoreDBSubscriptionToAll, EventStoreDBSubscriptionToAll>()
            .AddScoped<EventStoreDBExpectedStreamRevisionProvider, EventStoreDBExpectedStreamRevisionProvider>()
            .AddScoped<EventStoreDBNextStreamRevisionProvider, EventStoreDBNextStreamRevisionProvider>();

        if (options?.UseInternalCheckpointing != false)
        {
            services
                .AddTransient<ISubscriptionCheckpointRepository, EventStoreDBSubscriptionCheckpointRepository>();
        }

        return services;
    }
}
