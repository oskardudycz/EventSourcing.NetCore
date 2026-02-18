using Core.Commands;
using Core.Events;
using Core.Marten.Repository;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.DependencyInjection;
using Orders.Orders.CancellingOrder;
using Orders.Orders.CompletingOrder;
using Orders.Orders.GettingOrderStatus;
using Orders.Orders.GettingPending;
using Orders.Orders.InitializingOrder;
using Orders.Orders.RecordingOrderPayment;
using Orders.Payments.FinalizingPayment;
using Orders.Shipments.OutOfStockProduct;
using Orders.Shipments.SendingPackage;
using Orders.ShoppingCarts.FinalizingCart;

namespace Orders.Orders;
using static OrderEvent;
using static TimeHasPassed;

internal static class OrdersConfig
{
    internal static IServiceCollection AddOrders(this IServiceCollection services) =>
        services
            .AddMartenRepository<Order>()
            .AddCommandHandlers()
            .AddEventHandlers();

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services) =>
        services.AddCommandHandler<InitializeOrder, HandleInitializeOrder>()
            .AddCommandHandler<RecordOrderPayment, HandleRecordOrderPayment>()
            .AddCommandHandler<CompleteOrder, HandleCompleteOrder>()
            .AddCommandHandler<CancelOrder, HandleCancelOrder>();

    private static IServiceCollection AddEventHandlers(this IServiceCollection services) =>
        services.AddEventHandler<CartFinalized, OrderSaga>()
            .AddEventHandler<OrderInitiated, OrderSaga>()
            .AddEventHandler<PaymentFinalized, OrderSaga>()
            .AddEventHandler<PackageWasSent, OrderSaga>()
            .AddEventHandler<ProductWasOutOfStock, OrderSaga>()
            .AddEventHandler<OrderCancelled, OrderSaga>()
            .AddEventHandler<OrderPaymentRecorded, OrderSaga>()
            .AddEventHandler<MinuteHasPassed, HandleCancelOrder>();

    internal static void ConfigureOrders(this StoreOptions options)
    {
        // Snapshots
        options.Projections.LiveStreamAggregation<Order>();
        options.Projections.Add<OrderDetailsProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<PendingOrdersProjection>(ProjectionLifecycle.Inline);
    }
}
