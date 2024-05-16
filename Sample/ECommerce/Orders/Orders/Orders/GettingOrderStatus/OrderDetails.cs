using Marten.Events;
using Marten.Events.Aggregation;
using Orders.Orders.CancellingOrder;
using Orders.Orders.CompletingOrder;
using Orders.Orders.InitializingOrder;
using Orders.Orders.RecordingOrderPayment;
using Orders.Products;

namespace Orders.Orders.GettingOrderStatus;

public class OrderDetails
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }

    public IReadOnlyList<PricedProductItem> ProductItems { get; private set; } = default!;

    public decimal TotalPrice { get; private set; } = 0;

    public OrderStatus Status { get; private set; }

    public Guid? PaymentId { get; private set; }

    public List<EventsWrapper> Events = new();

    public DateTimeOffset InitiatedAt { get; private set; }

    public DateTimeOffset TimeoutAfter { get; private set; }

    public void Apply(IEvent<OrderInitiated> envelope)
    {
        var @event = envelope.Data;

        Id = @event.OrderId;
        ClientId = @event.ClientId;
        ProductItems = @event.ProductItems;
        Status = OrderStatus.Opened;
        InitiatedAt = @event.InitiatedAt;
        TimeoutAfter = @event.TimeoutAfter;

        Events.Add(EventsWrapper.From(envelope));
    }

    public void Apply(IEvent<OrderPaymentRecorded> envelope)
    {
        PaymentId = envelope.Data.PaymentId;
        Status = OrderStatus.Paid;

        Events.Add(EventsWrapper.From(envelope));
    }

    public void Apply(IEvent<OrderCompleted> envelope)
    {
        Status = OrderStatus.Completed;

        Events.Add(EventsWrapper.From(envelope));
    }

    public void Apply(IEvent<OrderCancelled> envelope)
    {
        Status = OrderStatus.Cancelled;

        Events.Add(EventsWrapper.From(envelope));
    }
}

public record EventsWrapper(int Version, string Type, object Event)
{
    public static EventsWrapper From(IEvent @event) => new((int)@event.Version, @event.EventTypeName, @event.Data);
}

public class OrderDetailsProjection: SingleStreamProjection<OrderDetails>
{
    public void Apply(IEvent<OrderInitiated> envelope, OrderDetails details) =>
        details.Apply(envelope);

    public void Apply(IEvent<OrderPaymentRecorded> envelope, OrderDetails details) =>
        details.Apply(envelope);

    public void Apply(IEvent<OrderCompleted> envelope, OrderDetails details) =>
        details.Apply(envelope);

    public void Apply(IEvent<OrderCancelled> envelope, OrderDetails details) =>
        details.Apply(envelope);
}
