using Core.Commands;
using Core.Marten.Repository;
using Core.Queries;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.DependencyInjection;
using SmartHome.Temperature.TemperatureMeasurements.GettingTemperatureMeasurements;
using SmartHome.Temperature.TemperatureMeasurements.RecordingTemperature;
using SmartHome.Temperature.TemperatureMeasurements.StartingTemperatureMeasurement;

namespace SmartHome.Temperature.TemperatureMeasurements;

public static class TemperatureSummaryConfig
{
    internal static IServiceCollection AddTemperatureMeasurements(this IServiceCollection services) =>
        services
            .AddMartenRepository<TemperatureMeasurement>()
            .AddCommandHandlers()
            .AddQueryHandlers();

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services) =>
        services.AddCommandHandler<StartTemperatureMeasurement, HandleStartTemperatureMeasurement>()
            .AddCommandHandler<RecordTemperature, HandleRecordTemperature>();

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services) =>
        services
            .AddQueryHandler<GetTemperatureMeasurements, IReadOnlyList<TemperatureMeasurement>,
                HandleGetTemperatureMeasurements>();

    internal static void ConfigureTemperatureMeasurements(this StoreOptions options)
    {
        // Snapshots
        options.Projections.Snapshot<TemperatureMeasurement>(SnapshotLifecycle.Async);
    }
}
