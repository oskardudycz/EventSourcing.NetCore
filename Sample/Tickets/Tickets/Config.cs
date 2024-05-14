using Core.Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tickets.Maintenance;
using Tickets.Reservations;

namespace Tickets;

public static class Config
{
    public static IServiceCollection AddTicketsModule(this IServiceCollection services, IConfiguration config) =>
        services
            .AddMarten(config, options =>
            {
                options.ConfigureReservations();
            })
            .Services
            .AddReservations()
            .AddMaintainance();
}
