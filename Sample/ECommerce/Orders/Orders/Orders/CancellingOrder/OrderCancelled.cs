using System;
using Core.Events;

namespace Orders.Orders.CancellingOrder;

public record OrderCancelled(
    Guid OrderId,
    Guid? PaymentId,
    OrderCancellationReason OrderCancellationReason,
    DateTime CancelledAt
): IEvent
{
    public static OrderCancelled Create(Guid orderId,
        Guid? paymentId,
        OrderCancellationReason orderCancellationReason,
        DateTime cancelledAt)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId));
        if (paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        if (orderCancellationReason == default)
            throw new ArgumentOutOfRangeException(nameof(orderCancellationReason));
        if (cancelledAt == default)
            throw new ArgumentOutOfRangeException(nameof(cancelledAt));

        return new OrderCancelled(orderId, paymentId, orderCancellationReason, cancelledAt);
    }
}
