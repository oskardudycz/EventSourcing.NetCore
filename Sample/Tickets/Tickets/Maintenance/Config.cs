using Core.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Tickets.Maintenance.Commands;
using Tickets.Reservations;

namespace Tickets.Maintenance
{
    internal static class Config
    {

        internal static void AddMaintainance(this IServiceCollection services)
        {
            AddCommandHandlers(services);
        }

        private static void AddCommandHandlers(IServiceCollection services)
        {
            services.AddCommandHandler<RebuildProjection, MaintenanceCommandHandler>();
        }
    }
}
