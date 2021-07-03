using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Orders.Shipments.SendingPackage;

namespace Orders.Shipments
{
    internal static class ShipmentsConfig
    {
        internal static IServiceCollection AddShipments(this IServiceCollection services)
        {
            return services.AddScoped<IRequestHandler<SendPackage, Unit>, HandleSendPackage>();
        }
    }
}
