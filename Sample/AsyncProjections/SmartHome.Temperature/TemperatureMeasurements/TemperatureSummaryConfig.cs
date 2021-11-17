using System.Collections.Generic;
using Core.Commands;
using Core.Marten.Repository;
using Core.Queries;
using Core.Repositories;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.DependencyInjection;
using SmartHome.Temperature.TemperatureMeasurements.GettingTemperatureMeasurements;
using SmartHome.Temperature.TemperatureMeasurements.RecordingTemperature;
using SmartHome.Temperature.TemperatureMeasurements.StartingTemperatureMeasurement;

namespace SmartHome.Temperature.TemperatureMeasurements;

public static class TemperatureSummaryConfig
{
    internal static void AddTemperatureMeasurements(this IServiceCollection services)
    {
        services.AddScoped<IRepository<TemperatureMeasurement>, MartenRepository<TemperatureMeasurement>>();

        AddCommandHandlers(services);
        AddQueryHandlers(services);
    }

    private static void AddCommandHandlers(IServiceCollection services)
    {
        services.AddCommandHandler<StartTemperatureMeasurement, HandleStartTemperatureMeasurement>()
            .AddCommandHandler<RecordTemperature, HandleRecordTemperature>();
    }

    private static void AddQueryHandlers(IServiceCollection services)
    {
        services.AddQueryHandler<GetTemperatureMeasurements, IReadOnlyList<TemperatureMeasurement>, HandleGetTemperatureMeasurements>();
    }

    internal static void ConfigureTemperatureMeasurements(this StoreOptions options)
    {
        // Snapshots
        options.Projections.SelfAggregate<TemperatureMeasurement>(ProjectionLifecycle.Async);
    }
}