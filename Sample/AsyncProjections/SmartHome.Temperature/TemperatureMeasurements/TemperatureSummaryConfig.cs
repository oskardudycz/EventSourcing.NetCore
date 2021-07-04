using System.Collections.Generic;
using Core.Marten.Repository;
using Core.Repositories;
using Marten;
using Marten.Events.Projections;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SmartHome.Temperature.TemperatureMeasurements.GettingTemperatureMeasurements;
using SmartHome.Temperature.TemperatureMeasurements.RecordingTemperature;
using SmartHome.Temperature.TemperatureMeasurements.StartingTemperatureMeasurement;

namespace SmartHome.Temperature.TemperatureMeasurements
{
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
            services.AddScoped<IRequestHandler<StartTemperatureMeasurement, Unit>, HandleStartTemperatureMeasurement>();
            services.AddScoped<IRequestHandler<RecordTemperature, Unit>, HandleRecordTemperature>();
        }

        private static void AddQueryHandlers(IServiceCollection services)
        {
            services
                .AddScoped<IRequestHandler<GetTemperatureMeasurements, IReadOnlyList<TemperatureMeasurement>>,
                    HandleGetTemperatureMeasurements>();
        }

        internal static void ConfigureTemperatureMeasurements(this StoreOptions options)
        {
            // Snapshots
            options.Projections.SelfAggregate<TemperatureMeasurement>(ProjectionLifecycle.Async);
        }
    }
}
