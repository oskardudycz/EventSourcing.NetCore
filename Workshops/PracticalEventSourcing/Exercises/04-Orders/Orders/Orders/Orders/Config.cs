using Orders.Orders.Commands;
using Core.Repositories;
using Core.Storage;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Orders.Carts.Events;
using Orders.Orders.Events;
using Orders.Payments.Events;
using Orders.Shipments.Events;

namespace Orders.Orders
{
    internal static class OrdersConfig
    {
        internal static IServiceCollection AddOrders(this IServiceCollection services)
        {
            return services;
        }

        private static IServiceCollection AddCommandHandlers(this IServiceCollection services)
        {
            return services;
        }

        private static IServiceCollection AddEventHandlers(this IServiceCollection services)
        {
            return services;
        }

        internal static void ConfigureOrders(this StoreOptions options)
        {
        }
    }
}
