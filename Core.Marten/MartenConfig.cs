using Core.Configuration;
using Core.Ids;
using Core.Marten.Commands;
using Core.Marten.Ids;
using Core.Marten.Repository;
using Core.Marten.Subscriptions;
using Core.OpenTelemetry;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weasel.Core;

namespace Core.Marten;

public class MartenConfig
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
        IConfiguration configuration,
        Action<StoreOptions>? configureOptions = null,
        string configKey = DefaultConfigKey,
        bool useExternalBus = false
    ) =>
        services.AddMarten(
            configuration.GetRequiredConfig<MartenConfig>(configKey),
            configureOptions,
            useExternalBus
        );

    public static IServiceCollection AddMarten(
        this IServiceCollection services,
        MartenConfig martenConfig,
        Action<StoreOptions>? configureOptions = null,
        bool useExternalBus = false
    )
    {
        services
            .AddScoped<IIdGenerator, MartenIdGenerator>()
            .AddMarten(sp => SetStoreOptions(sp, martenConfig, configureOptions))
            .ApplyAllDatabaseChangesOnStartup()
            //.OptimizeArtifactWorkflow()
            .AddAsyncDaemon(martenConfig.DaemonMode);

        if (useExternalBus)
            services.AddMartenAsyncCommandBus();

        return services;
    }

    private static StoreOptions SetStoreOptions(
        IServiceProvider serviceProvider,
        MartenConfig martenConfig,
        Action<StoreOptions>? configureOptions = null
    )
    {
        var options = new StoreOptions();
        options.Connection(martenConfig.ConnectionString);
        options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

        var schemaName = Environment.GetEnvironmentVariable("SchemaName");
        options.Events.DatabaseSchemaName = schemaName ?? martenConfig.WriteModelSchema;
        options.DatabaseSchemaName = schemaName ?? martenConfig.ReadModelSchema;

        options.UseDefaultSerialization(
            EnumStorage.AsString,
            nonPublicMembersStorage: NonPublicMembersStorage.All
        );

        options.Projections.Add(
            new MartenSubscription(
                new[]
                {
                    new MartenEventPublisher(
                        serviceProvider,
                        serviceProvider.GetRequiredService<IActivityScope>(),
                        serviceProvider.GetRequiredService<ILogger<MartenEventPublisher>>()
                    )
                },
                serviceProvider.GetRequiredService<ILogger<MartenSubscription>>()
            ),
            ProjectionLifecycle.Async,
            "MartenSubscription"
        );

        if (martenConfig.UseMetadata)
        {
            options.Events.MetadataConfig.CausationIdEnabled = true;
            options.Events.MetadataConfig.CorrelationIdEnabled = true;
            options.Events.MetadataConfig.HeadersEnabled = true;
        }

        configureOptions?.Invoke(options);

        return options;
    }
}
