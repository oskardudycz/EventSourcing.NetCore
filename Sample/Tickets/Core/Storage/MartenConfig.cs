using System;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Storage
{
    public class MartenConfig
    {
        private const string DefaultSchema = "public";

        public string ConnectionString { get; set; }

        public string WriteModelSchema { get; set; } = DefaultSchema;
        public string ReadModelSchema { get; set; } = DefaultSchema;
    }

    public static class MartenConfigExtensions
    {
        private const string DefaultConfigKey = "EventStore";

        public static void AddMarten(this IServiceCollection services, IConfiguration config, Action<StoreOptions> configureOptions = null)
        {
            services.AddSingleton<IDocumentStore>(sp => DocumentStore.For(options => SetStoreOptions(options, config, configureOptions)));
            services.AddScoped(sp => sp.GetRequiredService<IDocumentStore>().OpenSession());
        }

        private static void SetStoreOptions(StoreOptions options, IConfiguration configuration, Action<StoreOptions> configureOptions = null)
        {
            var config = configuration.GetSection(DefaultConfigKey).Get<MartenConfig>();

            options.Connection(config.ConnectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.Events.DatabaseSchemaName = config.WriteModelSchema;
            options.DatabaseSchemaName = config.ReadModelSchema;
            options.UseDefaultSerialization(nonPublicMembersStorage: NonPublicMembersStorage.NonPublicSetters);

            configureOptions?.Invoke(options);
        }
    }
}
