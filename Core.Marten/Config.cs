using System;
using Core.Ids;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Marten
{
    public class Config
    {
        private const string DefaultSchema = "public";

        public string ConnectionString { get; set; } = default!;

        public string WriteModelSchema { get; set; } = DefaultSchema;
        public string ReadModelSchema { get; set; } = DefaultSchema;

        public bool ShouldRecreateDatabase { get; set; } = false;

        public DaemonMode DaemonMode { get; set; } = DaemonMode.Disabled;
    }

    public static class MartenConfigExtensions
    {
        private const string DefaultConfigKey = "EventStore";

        public static IServiceCollection AddMarten(this IServiceCollection services, IConfiguration config,
            Action<StoreOptions>? configureOptions = null)
        {
            var martenConfig = config.GetSection(DefaultConfigKey).Get<Config>();

            services
                .AddScoped<IIdGenerator, MartenIdGenerator>();

            var documentStore = services
                .AddMarten(options =>
                {
                    SetStoreOptions(options, martenConfig, configureOptions);
                })
                .InitializeStore();

                if (martenConfig.ShouldRecreateDatabase)
                    documentStore.Advanced.Clean.CompletelyRemoveAll();

                documentStore.Schema.ApplyAllConfiguredChangesToDatabase();

            return services;
        }

        private static void SetStoreOptions(StoreOptions options, Config config,
            Action<StoreOptions>? configureOptions = null)
        {
            options.Connection(config.ConnectionString);
            options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
            options.Events.DatabaseSchemaName = config.WriteModelSchema;
            options.DatabaseSchemaName = config.ReadModelSchema;
            options.UseDefaultSerialization(nonPublicMembersStorage: NonPublicMembersStorage.NonPublicSetters,
                enumStorage: EnumStorage.AsString);
            options.PLV8Enabled = false;
            options.Events.Daemon.Mode = config.DaemonMode;

            configureOptions?.Invoke(options);
        }
    }
}
