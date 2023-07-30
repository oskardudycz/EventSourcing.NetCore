using Helpdesk.Core;
using Helpdesk.Core.Marten;
using Helpdesk.Incidents;
using Helpdesk.Incidents.GetIncidentDetails;
using Helpdesk.Incidents.GetIncidentHistory;
using Helpdesk.Incidents.GetIncidentShortInfo;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk;

public static class StorageConfiguration
{
    public static void AddMartenForHelpdeskInlineOnly(
        this IServiceCollection services,
        IConfiguration configuration
    ) =>
        services.AddMartenWithDefaults(
            configuration.GetHelpdeskConnectionString(),
            HelpdeskSchemaName,
            (options, _) =>
            {
                options.Projections.LiveStreamAggregation<Incident>();
                options.Projections.Add<IncidentHistoryTransformation>(ProjectionLifecycle.Inline);
                options.Projections.Add<IncidentDetailsProjection>(ProjectionLifecycle.Inline);
                options.Projections.Add<IncidentShortInfoProjection>(ProjectionLifecycle.Inline);
            });

    public static void AddMartenForHelpdeskAsyncOnly(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<StoreOptions, IServiceProvider> configure
    ) =>
        services.AddMartenAsyncOnly(
            configuration.GetHelpdeskConnectionString(),
            HelpdeskSchemaName,
            configure,
            configuration.GetValue<int>($"{HelpdeskSettingsSectionName}:{DaemonLockIdSectionName}")
        );

    private static string GetHelpdeskConnectionString(this IConfiguration configuration) =>
        configuration.GetConnectionString(IncidentsConnectionStringName) ?? throw new InvalidOperationException();

    private const string HelpdeskSchemaName = "Helpdesk";
    private const string IncidentsConnectionStringName = "Incidents";
    private const string HelpdeskSettingsSectionName = "Helpdesk";
    private const string DaemonLockIdSectionName = "DaemonLockId";
}
