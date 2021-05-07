using Core.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartHome.Temperature.MotionSensors;
using SmartHome.Temperature.TemperatureMeasurements;

namespace SmartHome.Temperature
{
    public static class Config
    {
        public static void AddTemperaturesModule(this IServiceCollection services, IConfiguration config)
        {
            services.AddMarten(config, options =>
            {
                options.ConfigureTemperatureMeasurements();
                options.ConfigureMotionSensors();
            });
            services.AddTemperatureMeasurements();
            services.AddMotionSensors();
        }
    }
}
