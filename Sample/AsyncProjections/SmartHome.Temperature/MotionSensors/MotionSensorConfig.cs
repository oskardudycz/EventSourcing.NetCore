using System.Collections.Generic;
using Core.Storage;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SmartHome.Temperature.MotionSensors.Commands;
using SmartHome.Temperature.MotionSensors.Queries;
using SmartHome.Temperature.TemperatureMeasurements;

namespace SmartHome.Temperature.MotionSensors
{
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
            services.AddScoped<IRequestHandler<InstallMotionSensor, Unit>, MotionSensorCommandHandler>();
            services.AddScoped<IRequestHandler<RebuildMotionSensorsViews, Unit>, MotionSensorCommandHandler>();
        }

        private static void AddQueryHandlers(IServiceCollection services)
        {
            services
                .AddScoped<IRequestHandler<GetMotionSensors, IReadOnlyList<MotionSensor>>,
                    MotionSensorQueryHandler>();
        }

        internal static void ConfigureMotionSensors(this StoreOptions options)
        {
            // Snapshots
            options.Events.InlineProjections.AggregateStreamsWith<MotionSensor>();
        }
    }
}
