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

        public bool ShouldRecreateDatabase { get; set; } = false;
    }

    public static class MartenConfigExtensions
    {
        private const string DefaultConfigKey = "EventStore";

        public static void AddMarten(this IServiceCollection services, IConfiguration config, Action<StoreOptions> configureOptions = null)
        {
            var martenConfig = config.GetSection(DefaultConfigKey).Get<MartenConfig>();

            services.AddSingleton<IDocumentStore>(sp =>
            {
                var documentStore = DocumentStore.For(options => SetStoreOptions(options, martenConfig, configureOptions));

                if (martenConfig.ShouldRecreateDatabase)
                    documentStore.Advanced.Clean.CompletelyRemoveAll();

                return documentStore;
            });
            services.AddScoped(sp => sp.GetRequiredService<IDocumentStore>().OpenSession());
            services.AddScoped(sp => sp.GetRequiredService<IDocumentStore>().QuerySession());
        }

        private static void SetStoreOptions(StoreOptions options, MartenConfig config, Action<StoreOptions> configureOptions = null)
        {
            options.Connection(config.ConnectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.Events.DatabaseSchemaName = config.WriteModelSchema;
            options.DatabaseSchemaName = config.ReadModelSchema;
            options.UseDefaultSerialization(nonPublicMembersStorage: NonPublicMembersStorage.NonPublicSetters);

            configureOptions?.Invoke(options);
        }
    }
}
