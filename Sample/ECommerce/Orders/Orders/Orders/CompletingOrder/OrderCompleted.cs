using System;
using Core.Events;

namespace Orders.Orders.CompletingOrder
{
    public class OrderCompleted : IEvent
    {
        public Guid OrderId { get; }

        public DateTime CompletedAt { get; }

        public OrderCompleted(Guid orderId, DateTime completedAt)
        {
            OrderId = orderId;
            CompletedAt = completedAt;
        }

        public static OrderCompleted Create(Guid orderId, DateTime completedAt)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(orderId));
            if (completedAt == default)
                throw new ArgumentOutOfRangeException(nameof(completedAt));

            return new OrderCompleted(orderId, completedAt);
        }
    }
}
