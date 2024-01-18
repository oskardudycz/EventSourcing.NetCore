using Core.Commands;
using Core.Events;
using Core.Marten.Repository;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.DependencyInjection;
using Orders.Orders.CancellingOrder;
using Orders.Orders.CompletingOrder;
using Orders.Orders.InitializingOrder;
using Orders.Orders.RecordingOrderPayment;
using Orders.Payments.FinalizingPayment;
using Orders.Shipments.OutOfStockProduct;
using Orders.Shipments.SendingPackage;
using Orders.ShoppingCarts.FinalizingCart;

namespace Orders.Orders;

internal static class OrdersConfig
{
    internal static IServiceCollection AddOrders(this IServiceCollection services)
    {
        return services
            .AddMartenRepository<Order>()
            .AddCommandHandlers()
            .AddEventHandlers();
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        return services.AddCommandHandler<InitializeOrder, HandleInitializeOrder>()
            .AddCommandHandler<RecordOrderPayment, HandleRecordOrderPayment>()
            .AddCommandHandler<CompleteOrder, HandleCompleteOrder>()
            .AddCommandHandler<CancelOrder, HandleCancelOrder>();
    }

    private static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        return services.AddEventHandler<CartFinalized, OrderSaga>()
            .AddEventHandler<OrderInitialized, OrderSaga>()
            .AddEventHandler<PaymentFinalized, OrderSaga>()
            .AddEventHandler<PackageWasSent, OrderSaga>()
            .AddEventHandler<ProductWasOutOfStock, OrderSaga>()
            .AddEventHandler<OrderCancelled, OrderSaga>()
            .AddEventHandler<OrderPaymentRecorded, OrderSaga>();
    }

    internal static void ConfigureOrders(this StoreOptions options)
    {
        // Snapshots
        options.Projections.Snapshot<Order>(SnapshotLifecycle.Inline);
    }
}
