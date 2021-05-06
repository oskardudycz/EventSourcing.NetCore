using System;
using Core.Events;

namespace Orders.Payments.Events
{
    public class PaymentFailed: IExternalEvent
    {
        public Guid OrderId { get; }

        public Guid PaymentId { get; }

        public decimal Amount { get; }

        public DateTime FailedAt { get; }

        public PaymentFailed(Guid orderId, Guid paymentId, decimal amount, DateTime failedAt)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            Amount = amount;
            FailedAt = failedAt;
        }
    }
}
