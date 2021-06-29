using System;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Core.EventStoreDB
{
    public class Config
    {
        public string ConnectionString { get; set; } = default!;
    }

    public static class EventStoreDBConfigExtensions
    {
        private const string DefaultConfigKey = "EventStore";

        public static IServiceCollection AddEventStoreDB(this IServiceCollection services, IConfiguration config)
        {
            var eventStoreDBConfig = config.GetSection(DefaultConfigKey).Get<Config>();


            services.AddSingleton(
                new EventStoreClient(EventStoreClientSettings.Create(eventStoreDBConfig.ConnectionString)));

            return services;
        }
    }
}
