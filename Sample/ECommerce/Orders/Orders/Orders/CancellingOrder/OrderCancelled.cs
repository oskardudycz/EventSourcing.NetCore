using System;
using Core.Events;

namespace Orders.Orders.CancellingOrder
{
    public class OrderCancelled: IEvent
    {
        public Guid OrderId { get; }
        public Guid? PaymentId { get; }
        public OrderCancellationReason OrderCancellationReason { get; }
        public DateTime CancelledAt { get; }

        public OrderCancelled(Guid orderId,
            Guid? paymentId,
            OrderCancellationReason orderCancellationReason,
            DateTime cancelledAt)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            OrderCancellationReason = orderCancellationReason;
            CancelledAt = cancelledAt;
        }

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
}
