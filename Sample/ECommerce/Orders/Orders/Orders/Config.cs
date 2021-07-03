using Core.Marten.Repository;
using Core.Repositories;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Orders.Carts.FinalizingCart;
using Orders.Orders.CancellingOrder;
using Orders.Orders.CompletingOrder;
using Orders.Orders.InitializingOrder;
using Orders.Orders.RecordingOrderPayment;
using Orders.Payments.FinalizingPayment;
using Orders.Shipments.OutOfStockProduct;
using Orders.Shipments.SendingPackage;

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
            return services.AddScoped<IRequestHandler<InitializeOrder, Unit>, HandleInitializeOrder>()
                .AddScoped<IRequestHandler<RecordOrderPayment, Unit>, HandleRecordOrderPayment>()
                .AddScoped<IRequestHandler<CompleteOrder, Unit>, HandleCompleteOrder>()
                .AddScoped<IRequestHandler<CancelOrder, Unit>, HandleCancelOrder>();
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
