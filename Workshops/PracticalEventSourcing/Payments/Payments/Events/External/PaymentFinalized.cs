using System;
using Core.Events;

namespace Payments.Payments.Events.External
{
    public class PaymentFinalized: IExternalEvent
    {
        public Guid OrderId { get; }

        public Guid PaymentId { get; }

        public decimal Amount { get; }

        public DateTime FinalizedAt { get; }

        public PaymentFinalized(Guid orderId, Guid paymentId, decimal amount, DateTime finalizedAt)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            Amount = amount;
            FinalizedAt = finalizedAt;
        }
    }
}
