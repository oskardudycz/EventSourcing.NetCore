using Marten.Events.Aggregation;

namespace Orders.Orders.GettingPending;
using static OrderEvent;

public class PendingOrder
{
    public Guid Id { get; private set; }

    public DateTimeOffset TimeoutAfter { get; private set; }

    public void Apply(OrderInitiated @event) =>
        TimeoutAfter = @event.TimeoutAfter;
}


public class PendingOrdersProjection: SingleStreamProjection<PendingOrder, string>
{
    public PendingOrdersProjection()
    {
        DeleteEvent<OrderCompleted>();
        DeleteEvent<OrderCancelled>();
    }

    public void Apply(OrderInitiated @event, PendingOrder details) =>
        details.Apply(@event);
}
