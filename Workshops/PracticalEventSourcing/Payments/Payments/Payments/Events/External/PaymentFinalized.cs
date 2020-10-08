using System;
using Ardalis.GuardClauses;
using Core.Events;

namespace Payments.Payments.Events.External
{
    public class PaymentFinalized: IExternalEvent
    {
        public Guid OrderId { get; }

        public Guid PaymentId { get; }

        public decimal Amount { get; }

        public DateTime FinalizedAt { get; }

        private PaymentFinalized(Guid paymentId, Guid orderId, decimal amount, DateTime finalizedAt)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            Amount = amount;
            FinalizedAt = finalizedAt;
        }
        public static PaymentFinalized Create(Guid paymentId, Guid orderId, decimal amount, DateTime finalizedAt)
        {
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.Default(paymentId, nameof(paymentId));
            Guard.Against.NegativeOrZero(amount, nameof(amount));
            Guard.Against.Default(finalizedAt, nameof(finalizedAt));

            return new PaymentFinalized(paymentId, orderId, amount, finalizedAt);
        }
    }
}
