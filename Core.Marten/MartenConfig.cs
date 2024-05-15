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
using Marten.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
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

    public static MartenServiceCollectionExtensions.MartenConfigurationExpression AddMarten(
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

    public static MartenServiceCollectionExtensions.MartenConfigurationExpression AddMarten(
        this IServiceCollection services,
        MartenConfig martenConfig,
        Action<StoreOptions>? configureOptions = null,
        bool useExternalBus = false
    )
    {
        var config = services
            .AddScoped<IIdGenerator, MartenIdGenerator>()
            .AddMarten(sp =>
            {
                var dataSource = sp.GetService<NpgsqlDataSource>();
                if (dataSource != null)
                {
                    martenConfig.ConnectionString = dataSource.ConnectionString;
                    Console.WriteLine(dataSource.ConnectionString);
                }

                return SetStoreOptions(martenConfig, configureOptions);
            })
            .UseLightweightSessions()
            .ApplyAllDatabaseChangesOnStartup()
            //.OptimizeArtifactWorkflow()
            .AddAsyncDaemon(martenConfig.DaemonMode)
            .AddSubscriptionWithServices<MartenEventPublisher>(ServiceLifetime.Scoped);

        if (useExternalBus)
            services.AddMartenAsyncCommandBus();


        return config;
    }

    private static StoreOptions SetStoreOptions(
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

        options.UseNewtonsoftForSerialization(
            EnumStorage.AsString,
            nonPublicMembersStorage: NonPublicMembersStorage.All
        );

        options.Projections.Errors.SkipApplyErrors = false;
        options.Projections.Errors.SkipSerializationErrors = false;
        options.Projections.Errors.SkipUnknownEvents = false;

        if (martenConfig.UseMetadata)
        {
            options.Events.MetadataConfig.CausationIdEnabled = true;
            options.Events.MetadataConfig.CorrelationIdEnabled = true;
            options.Events.MetadataConfig.HeadersEnabled = true;
        }

        // Turn on Otel tracing for connection activity, and
        // also tag events to each span for all the Marten "write"
        // operations
        options.OpenTelemetry.TrackConnections = TrackLevel.Normal;

        // This opts into exporting a counter just on the number
        // of events being appended. Kinda a duplication
        options.OpenTelemetry.TrackEventCounters();

        configureOptions?.Invoke(options);

        return options;
    }
}
