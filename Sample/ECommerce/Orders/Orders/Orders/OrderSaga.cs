using Core.Commands;
using Core.Events;
using Core.Ids;
using Orders.Orders.CancellingOrder;
using Orders.Orders.CompletingOrder;
using Orders.Orders.InitializingOrder;
using Orders.Orders.RecordingOrderPayment;
using Orders.Payments.DiscardingPayment;
using Orders.Payments.FinalizingPayment;
using Orders.Payments.RequestingPayment;
using Orders.Products;
using Orders.Shipments.OutOfStockProduct;
using Orders.Shipments.SendingPackage;
using Orders.ShoppingCarts.FinalizingCart;

namespace Orders.Orders;

public class OrderSaga(IIdGenerator idGenerator, ICommandBus commandBus):
    IEventHandler<CartFinalized>,
    IEventHandler<OrderInitiated>,
    IEventHandler<PaymentFinalized>,
    IEventHandler<OrderPaymentRecorded>,
    IEventHandler<PackageWasSent>,
    IEventHandler<ProductWasOutOfStock>,
    IEventHandler<OrderCancelled>
{
    // Happy path
    public async Task Handle(CartFinalized @event, CancellationToken ct)
    {
        var orderId = idGenerator.New();

        await commandBus.Send(
            InitializeOrder.Create(orderId, @event.ClientId, @event.ProductItems, @event.TotalPrice),
            ct
        );
    }

    public async Task Handle(OrderInitiated @event, CancellationToken ct)
    {
        await commandBus.Send(
            RequestPayment.Create(@event.OrderId, @event.TotalPrice),
            ct
        );
    }

    public async Task Handle(PaymentFinalized @event, CancellationToken ct)
    {
        await commandBus.Send(
            RecordOrderPayment.Create(@event.OrderId, @event.PaymentId, @event.FinalizedAt),
            ct
        );
    }

    public async Task Handle(OrderPaymentRecorded @event, CancellationToken ct)
    {
        await commandBus.Send(
            SendPackage.Create(
                @event.OrderId,
                @event.ProductItems.Select(pi => new ProductItem(pi.ProductId, pi.Quantity)).ToList()
            ),
            ct
        );
    }

    public async Task Handle(PackageWasSent @event, CancellationToken ct)
    {
        await commandBus.Send(
            CompleteOrder.For(@event.OrderId),
            ct
        );
    }

    // Compensation
    public async Task Handle(ProductWasOutOfStock @event, CancellationToken ct)
    {
        await commandBus.Send(
            CancelOrder.Create(@event.OrderId, OrderCancellationReason.ProductWasOutOfStock),
            ct
        );
    }

    public async Task Handle(OrderCancelled @event, CancellationToken ct)
    {
        if (!@event.PaymentId.HasValue)
            return;

        await commandBus.Send(
            DiscardPayment.Create(@event.PaymentId.Value),
            ct
        );
    }
}
