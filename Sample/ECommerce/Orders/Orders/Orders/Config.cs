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
            return services.AddScoped<IRepository<Order>, MartenRepository<Order>>()
                .AddCommandHandlers()
                .AddEventHandlers();
        }

        private static IServiceCollection AddCommandHandlers(this IServiceCollection services)
        {
            return services.AddScoped<IRequestHandler<InitOrder, Unit>, OrderCommandHandler>()
                .AddScoped<IRequestHandler<RecordOrderPayment, Unit>, OrderCommandHandler>()
                .AddScoped<IRequestHandler<CompleteOrder, Unit>, OrderCommandHandler>()
                .AddScoped<IRequestHandler<CancelOrder, Unit>, OrderCommandHandler>();
        }

        private static IServiceCollection AddEventHandlers(this IServiceCollection services)
        {
            return services.AddScoped<INotificationHandler<CartFinalized>, OrderSaga>()
                .AddScoped<INotificationHandler<OrderInitialized>, OrderSaga>()
                .AddScoped<INotificationHandler<PaymentFinalized>, OrderSaga>()
                .AddScoped<INotificationHandler<PackageWasSent>, OrderSaga>()
                .AddScoped<INotificationHandler<ProductWasOutOfStock>, OrderSaga>()
                .AddScoped<INotificationHandler<OrderCancelled>, OrderSaga>()
                .AddScoped<INotificationHandler<OrderPaymentRecorded>, OrderSaga>();
        }

        internal static void ConfigureOrders(this StoreOptions options)
        {
            // Snapshots
            options.Projections.SelfAggregate<Order>();
        }
    }
}
