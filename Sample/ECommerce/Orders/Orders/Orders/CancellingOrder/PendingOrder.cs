using Marten.Events.Aggregation;
using Orders.Orders.CompletingOrder;
using Orders.Orders.InitializingOrder;

namespace Orders.Orders.CancellingOrder;

public class PendingOrder
{
    public Guid Id { get; private set; }

    public DateTimeOffset TimeoutAfter { get; private set; }

    public void Apply(OrderInitiated @event) =>
        TimeoutAfter = @event.TimeoutAfter;
}


public class PendingOrdersProjection: SingleStreamProjection<PendingOrder>
{
    public PendingOrdersProjection()
    {
        DeleteEvent<OrderCompleted>();
        DeleteEvent<OrderCancelled>();
    }

    public void Apply(OrderInitiated @event, PendingOrder details) =>
        details.Apply(@event);
}
