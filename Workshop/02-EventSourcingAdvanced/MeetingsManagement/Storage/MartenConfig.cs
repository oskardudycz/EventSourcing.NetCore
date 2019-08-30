using System;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsManagement.Storage
{
    public class MartenConfig
    {
        public const string DefaultSchema = "public";

        public string ConnectionString { get; set; }

        public string WriteModelSchema { get; set; } = DefaultSchema;
        public string ReadModelSchema { get; set; } = DefaultSchema;
    }

    internal static class MartenConfigExtensions
    {
        public const string DefaultConfigKey = "EventStore";

        internal static void AddMarten(this IServiceCollection services, IConfiguration config, Action<StoreOptions> configureOptions = null)
        {
            services.AddSingleton<IDocumentStore>(sp => DocumentStore.For(options => SetStoreOptions(options, config)));
            services.AddScoped(sp => sp.GetRequiredService<IDocumentStore>().OpenSession());
        }

        private static void SetStoreOptions(StoreOptions options, IConfiguration configuration, Action<StoreOptions> configureOptions = null)
        {
            var config = configuration.GetSection(DefaultConfigKey).Get<MartenConfig>();

            options.Connection(config.ConnectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.Events.DatabaseSchemaName = config.WriteModelSchema;
            options.DatabaseSchemaName = config.ReadModelSchema;

            configureOptions?.Invoke(options);
        }
    }
}
