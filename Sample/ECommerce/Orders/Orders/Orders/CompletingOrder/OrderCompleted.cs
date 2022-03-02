using System;
using Core.Events;

namespace Orders.Orders.CompletingOrder;

public record OrderCompleted(
    Guid OrderId,
    DateTime CompletedAt
): IEvent
{
    public static OrderCompleted Create(Guid orderId, DateTime completedAt)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId));
        if (completedAt == default)
            throw new ArgumentOutOfRangeException(nameof(completedAt));

        return new OrderCompleted(orderId, completedAt);
    }
}
