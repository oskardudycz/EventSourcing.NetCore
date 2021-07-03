using System;
using Ardalis.GuardClauses;
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
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.Default(completedAt, nameof(completedAt));

            return new OrderCompleted(orderId, completedAt);
        }
    }
}
