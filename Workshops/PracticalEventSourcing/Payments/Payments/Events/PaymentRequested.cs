using System;
using Core.Events;
using Payments.Payments.Events.Enums;

namespace Payments.Payments.Events
{
    public class PaymentRequested: IEvent
    {
        public Guid OrderId { get; }

        public Guid PaymentId { get; }

        public decimal Amount { get; }

        public PaymentRequested(Guid orderId, Guid paymentId, decimal amount)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            Amount = amount;
        }
    }
}
