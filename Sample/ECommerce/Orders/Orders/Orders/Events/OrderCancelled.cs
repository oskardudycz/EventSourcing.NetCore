using System;
using Ardalis.GuardClauses;
using Core.Events;
using Orders.Orders.Enums;

namespace Orders.Orders.Events
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
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.Default(paymentId, nameof(paymentId));
            Guard.Against.Default(orderCancellationReason, nameof(orderCancellationReason));
            Guard.Against.Default(cancelledAt, nameof(cancelledAt));

            return new OrderCancelled(orderId, paymentId, orderCancellationReason, cancelledAt);
        }
    }
}
