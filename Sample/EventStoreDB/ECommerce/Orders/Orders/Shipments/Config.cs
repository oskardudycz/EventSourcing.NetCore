using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Orders.Shipments.Commands;

namespace Orders.Shipments
{
    internal static class ShipmentsConfig
    {
        internal static IServiceCollection AddShipments(this IServiceCollection services)
        {
            return services.AddScoped<IRequestHandler<SendPackage, Unit>, ShipmentsCommandHandler>();
        }
    }
}
