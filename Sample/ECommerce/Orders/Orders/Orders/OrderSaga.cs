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

public class OrderSaga:
    IEventHandler<CartFinalized>,
    IEventHandler<OrderInitialized>,
    IEventHandler<PaymentFinalized>,
    IEventHandler<PackageWasSent>,
    IEventHandler<ProductWasOutOfStock>,
    IEventHandler<OrderCancelled>,
    IEventHandler<OrderPaymentRecorded>
{
    private readonly IIdGenerator idGenerator;
    private readonly ICommandBus commandBus;

    public OrderSaga(IIdGenerator idGenerator, ICommandBus commandBus)
    {
        this.idGenerator = idGenerator;
        this.commandBus = commandBus;
    }

    // Happy path
    public Task Handle(CartFinalized @event, CancellationToken cancellationToken)
    {
        var orderId = idGenerator.New();

        return commandBus.Send(InitializeOrder.Create(orderId, @event.ClientId, @event.ProductItems, @event.TotalPrice));
    }

    public Task Handle(OrderInitialized @event, CancellationToken cancellationToken)
    {
        return commandBus.Send(RequestPayment.Create(@event.OrderId, @event.TotalPrice));
    }

    public async Task Handle(PaymentFinalized @event, CancellationToken cancellationToken)
    {
        await commandBus.Send(RecordOrderPayment.Create(@event.OrderId, @event.PaymentId, @event.FinalizedAt));
    }

    public Task Handle(OrderPaymentRecorded @event, CancellationToken cancellationToken)
    {
        return commandBus.Send(
            SendPackage.Create(
                @event.OrderId,
                @event.ProductItems.Select(pi => new ProductItem(pi.ProductId, pi.Quantity)).ToList()
            )
        );
    }

    public Task Handle(PackageWasSent @event, CancellationToken cancellationToken)
    {
        return commandBus.Send(CompleteOrder.Create(@event.OrderId));
    }

    // Compensation
    public Task Handle(ProductWasOutOfStock @event, CancellationToken cancellationToken)
    {
        return commandBus.Send(CancelOrder.Create(@event.OrderId, (OrderCancellationReason) OrderCancellationReason.ProductWasOutOfStock));
    }

    public Task Handle(OrderCancelled @event, CancellationToken cancellationToken)
    {
        if (!@event.PaymentId.HasValue)
            return Task.CompletedTask;

        return commandBus.Send(DiscardPayment.Create(@event.PaymentId.Value));
    }
}
