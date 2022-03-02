using Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Tickets.Maintenance.Commands;

namespace Tickets.Maintenance;

internal static class Config
{
    internal static IServiceCollection AddMaintainance(this IServiceCollection services) =>
        services.AddCommandHandlers();

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services) =>
        services.AddCommandHandler<RebuildProjection, MaintenanceCommandHandler>();
}
