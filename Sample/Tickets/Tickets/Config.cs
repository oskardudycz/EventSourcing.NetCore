using Core.Marten;
using JasperFx.Events;
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
                options.Events.StreamIdentity = StreamIdentity.AsGuid;
                options.ConfigureReservations();
            })
            .Services
            .AddReservations()
            .AddMaintainance();
}
