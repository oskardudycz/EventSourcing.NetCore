using System;
using Core.Events;

namespace Orders.Orders.Events
{
    public class OrderCancelled: IEvent
    {
        public Guid OrderId { get; }
        public Guid PaymentId { get; }

        public DateTime CancelledAt { get; }

        public OrderCancelled(Guid orderId, Guid paymentId, DateTime cancelledAt)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            CancelledAt = cancelledAt;
        }
    }
}
