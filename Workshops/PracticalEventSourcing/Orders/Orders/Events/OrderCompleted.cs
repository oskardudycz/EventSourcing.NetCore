using System;
using Core.Events;

namespace Orders.Orders.Events
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

    }
}
