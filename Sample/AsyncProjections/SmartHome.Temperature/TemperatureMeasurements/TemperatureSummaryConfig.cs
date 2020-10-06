using System.Collections.Generic;
using Core.Repositories;
using Core.Storage;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SmartHome.Temperature.TemperatureMeasurements.Commands;
using SmartHome.Temperature.TemperatureMeasurements.Queries;

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
            services.AddScoped<IRequestHandler<StartTemperatureMeasurement, Unit>, TemperatureSummaryCommandHandler>();
            services.AddScoped<IRequestHandler<RecordTemperature, Unit>, TemperatureSummaryCommandHandler>();
        }

        private static void AddQueryHandlers(IServiceCollection services)
        {
            services
                .AddScoped<IRequestHandler<GetTemperatureMeasurements, IReadOnlyList<TemperatureMeasurement>>,
                    TemperatureSummaryQueryHandler>();
        }

        internal static void ConfigureTemperatureMeasurements(this StoreOptions options)
        {
            // Snapshots
            options.Events.AsyncProjections.AggregateStreamsWith<TemperatureMeasurement>();
        }
    }
}
