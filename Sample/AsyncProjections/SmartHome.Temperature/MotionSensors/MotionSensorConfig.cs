using Core.Commands;
using Core.Marten.Repository;
using Core.Queries;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using SmartHome.Temperature.MotionSensors.GettingMotionSensor;
using SmartHome.Temperature.MotionSensors.InstallingMotionSensor;
using SmartHome.Temperature.MotionSensors.RebuildingMotionSensorsViews;

namespace SmartHome.Temperature.MotionSensors;

public static class MotionSensorConfig
{
    internal static IServiceCollection AddMotionSensors(this IServiceCollection services) =>
        services
            .AddMartenRepository<MotionSensor>()
            .AddCommandHandlers()
            .AddQueryHandlers();

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services) =>
        services.AddCommandHandler<InstallMotionSensor, HandleInstallMotionSensor>()
            .AddCommandHandler<RebuildMotionSensorsViews, HandleRebuildMotionSensorsViews>();

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services) =>
        services.AddQueryHandler<GetMotionSensors, IReadOnlyList<MotionSensor>, HandleGetMotionSensors>();

    internal static void ConfigureMotionSensors(this StoreOptions options)
    {
        // Snapshots
        options.Projections.SelfAggregate<MotionSensor>();
    }
}
