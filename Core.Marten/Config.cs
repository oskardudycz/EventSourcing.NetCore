using Core.Ids;
using Core.Marten.Events;
using Core.Marten.Ids;
using Core.Marten.Subscriptions;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weasel.Core;

namespace Core.Marten;

public class Config
{
    private const string DefaultSchema = "public";

    public string ConnectionString { get; set; } = default!;

    public string WriteModelSchema { get; set; } = DefaultSchema;
    public string ReadModelSchema { get; set; } = DefaultSchema;

    public bool ShouldRecreateDatabase { get; set; } = false;

    public DaemonMode DaemonMode { get; set; } = DaemonMode.Solo;

    public bool UseMetadata = true;
}

public static class MartenConfigExtensions
{
    private const string DefaultConfigKey = "EventStore";

    public static IServiceCollection AddMarten(
        this IServiceCollection services,
        IConfiguration config,
        Action<StoreOptions>? configureOptions = null,
        string configKey = DefaultConfigKey
    )
    {
        var martenConfig = config.GetSection(configKey).Get<Config>();

        services
            .AddScoped<IIdGenerator, MartenIdGenerator>()
            .AddScoped<IMartenAppendScope, MartenAppendScope>()
            .AddMartenAppendScope()
            .AddMarten(sp => SetStoreOptions(sp, martenConfig, configureOptions))
            .ApplyAllDatabaseChangesOnStartup()
            .AddAsyncDaemon(DaemonMode.Solo);

        return services;
    }

    private static StoreOptions SetStoreOptions(
        IServiceProvider serviceProvider,
        Config config,
        Action<StoreOptions>? configureOptions = null
    )
    {
        var options = new StoreOptions();
        options.Connection(config.ConnectionString);
        options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

        var schemaName = Environment.GetEnvironmentVariable("SchemaName");
        options.Events.DatabaseSchemaName = schemaName ?? config.WriteModelSchema;
        options.DatabaseSchemaName = schemaName ?? config.ReadModelSchema;

        options.UseDefaultSerialization(
            EnumStorage.AsString,
            nonPublicMembersStorage: NonPublicMembersStorage.All
        );

        options.Projections.Add(
            new MartenSubscription(
                new[] { new MartenEventPublisher(serviceProvider) },
                serviceProvider.GetRequiredService<ILogger<MartenSubscription>>()
            ),
            ProjectionLifecycle.Async,
            "MartenSubscription"
        );

        configureOptions?.Invoke(options);

        if (config.UseMetadata)
        {
            options.Events.MetadataConfig.CausationIdEnabled = true;
            options.Events.MetadataConfig.CorrelationIdEnabled = true;
        }

        return options;
    }
}
