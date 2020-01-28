using Core.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tickets.Maintenance;
using Tickets.Reservations;

namespace Tickets
{
    public static class Config
    {
        public static void AddTicketsModule(this IServiceCollection services, IConfiguration config)
        {
            services.AddMarten(config, options =>
            {
                options.ConfigureReservations();
            });
            services.AddReservations();
            services.AddMaintainance();
        }
    }
}
