using System;
using Ardalis.GuardClauses;
using Core.Events;

namespace Orders.Orders.Events
{
    public class OrderCancelled: IEvent
    {
        public Guid OrderId { get; }
        public Guid? PaymentId { get; }

        public DateTime CancelledAt { get; }

        public OrderCancelled(
            Guid orderId,
            Guid? paymentId,
            DateTime cancelledAt
        )
        {
            OrderId = orderId;
            PaymentId = paymentId;
            CancelledAt = cancelledAt;
        }

        public static OrderCancelled Create(
            Guid orderId,
            Guid? paymentId,
            DateTime cancelledAt
        )
        {
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.Default(paymentId, nameof(paymentId));
            Guard.Against.Default(cancelledAt, nameof(cancelledAt));

            return new OrderCancelled(orderId, paymentId, cancelledAt);
        }
    }
}
