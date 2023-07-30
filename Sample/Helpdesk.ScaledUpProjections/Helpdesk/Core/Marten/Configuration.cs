using JasperFx.CodeGeneration;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Services.Json;
using Microsoft.Extensions.DependencyInjection;
using Weasel.Core;

namespace Helpdesk.Core.Marten;

public static class Configuration
{
    public static void AddMartenAsyncOnly(
        this IServiceCollection services,
        string connectionString,
        string schemaName,
        Action<StoreOptions, IServiceProvider> configure,
        int? daemonLockId = null
    ) =>
        services.AddMartenWithDefaults(connectionString, schemaName, (options, sp) =>
            {
                if (daemonLockId.HasValue)
                    options.Projections.DaemonLockId = daemonLockId.Value;

                configure(options, sp);
            })
            .AddAsyncDaemon(DaemonMode.HotCold);

    public static MartenServiceCollectionExtensions.MartenConfigurationExpression AddMartenWithDefaults(
        this IServiceCollection services,
        string connectionString,
        string schemaName,
        Action<StoreOptions, IServiceProvider> configure
    ) =>
        services.AddMarten(sp =>
            {
                var options = new StoreOptions();
                schemaName = Environment.GetEnvironmentVariable("SchemaName") ?? schemaName;
                options.Events.DatabaseSchemaName = schemaName;
                options.DatabaseSchemaName = schemaName;
                options.Connection(connectionString);

                options.UseDefaultSerialization(
                    EnumStorage.AsString,
                    nonPublicMembersStorage: NonPublicMembersStorage.All,
                    serializerType: SerializerType.SystemTextJson
                );

                configure(options, sp);

                return options;
            })
            .OptimizeArtifactWorkflow(TypeLoadMode.Static)
            .UseLightweightSessions();
}
