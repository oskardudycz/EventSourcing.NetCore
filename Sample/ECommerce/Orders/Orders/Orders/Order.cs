using Core.Aggregates;
using Orders.Orders.CancellingOrder;
using Orders.Orders.CompletingOrder;
using Orders.Orders.InitializingOrder;
using Orders.Orders.RecordingOrderPayment;
using Orders.Products;

namespace Orders.Orders;

public class Order: Aggregate
{
    public readonly TimeSpan DefaultTimeOut = TimeSpan.FromMinutes(5);
    public Guid? ClientId { get; private set; }

    public IReadOnlyList<PricedProductItem> ProductItems { get; private set; } = default!;

    public decimal TotalPrice { get; private set; } = 0;

    public OrderStatus Status { get; private set; }

    public Guid? PaymentId { get; private set; }

    public static Order Initialize(
        Guid orderId,
        Guid clientId,
        IReadOnlyList<PricedProductItem> productItems,
        decimal totalPrice,
        DateTimeOffset now
    ) =>
        new(
            orderId,
            clientId,
            productItems,
            totalPrice,
            now
        );

    public Order() { }

    private Order(
        Guid id,
        Guid clientId,
        IReadOnlyList<PricedProductItem> productItems,
        decimal totalPrice,
        DateTimeOffset now,
        TimeSpan? timeout = null
    )
    {
        var @event = OrderInitiated.From(
            id,
            clientId,
            productItems,
            totalPrice,
            now,
            now.Add(timeout ?? DefaultTimeOut)
        );

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(OrderInitiated @event)
    {
        Id = @event.OrderId;
        ClientId = @event.ClientId;
        ProductItems = @event.ProductItems;
        Status = OrderStatus.Opened;
    }

    public void RecordPayment(Guid paymentId, DateTimeOffset recordedAt)
    {
        var @event = OrderPaymentRecorded.Create(
            Id,
            paymentId,
            ProductItems,
            TotalPrice,
            recordedAt
        );

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(OrderPaymentRecorded @event)
    {
        PaymentId = @event.PaymentId;
        Status = OrderStatus.Paid;
    }

    public void Complete(DateTimeOffset now)
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException($"Cannot complete a not paid order.");

        var @event = OrderCompleted.Create(Id, now);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(OrderCompleted @event)
    {
        Status = OrderStatus.Completed;
    }

    public void Cancel(OrderCancellationReason cancellationReason)
    {
        if (OrderStatus.Closed.HasFlag(Status))
            throw new InvalidOperationException($"Cannot cancel a closed order.");

        var @event = OrderCancelled.Create(Id, PaymentId, cancellationReason, DateTimeOffset.UtcNow);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(OrderCancelled @event)
    {
        Status = OrderStatus.Cancelled;
    }
}
