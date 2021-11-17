using System.Collections.Generic;
using Core.Commands;
using Core.Marten.Repository;
using Core.Queries;
using Core.Repositories;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using SmartHome.Temperature.MotionSensors.GettingMotionSensor;
using SmartHome.Temperature.MotionSensors.InstallingMotionSensor;
using SmartHome.Temperature.MotionSensors.RebuildingMotionSensorsViews;

namespace SmartHome.Temperature.MotionSensors;

public static class MotionSensorConfig
{
    internal static void AddMotionSensors(this IServiceCollection services)
    {
        services.AddScoped<IRepository<MotionSensor>, MartenRepository<MotionSensor>>();

        AddCommandHandlers(services);
        AddQueryHandlers(services);
    }

    private static void AddCommandHandlers(IServiceCollection services)
    {
        services.AddCommandHandler<InstallMotionSensor, HandleInstallMotionSensor>()
            .AddCommandHandler<RebuildMotionSensorsViews, HandleRebuildMotionSensorsViews>();
    }

    private static void AddQueryHandlers(IServiceCollection services)
    {
        services.AddQueryHandler<GetMotionSensors, IReadOnlyList<MotionSensor>, HandleGetMotionSensors>();
    }

    internal static void ConfigureMotionSensors(this StoreOptions options)
    {
        // Snapshots
        options.Projections.SelfAggregate<MotionSensor>();
    }
}