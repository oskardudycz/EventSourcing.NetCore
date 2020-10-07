using System;
using Ardalis.GuardClauses;
using Core.Events;
using Payments.Payments.Events.Enums;

namespace Payments.Payments.Events
{
    public class PaymentRequested: IEvent
    {
        public Guid OrderId { get; }

        public Guid PaymentId { get; }

        public decimal Amount { get; }

        private PaymentRequested(Guid orderId, Guid paymentId, decimal amount)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            Amount = amount;
        }

        public static PaymentRequested Create(Guid paymentId, Guid orderId, in decimal amount)
        {
            Guard.Against.Default(paymentId, nameof(paymentId));
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.NegativeOrZero(amount, nameof(amount));

            return new PaymentRequested(paymentId, orderId, amount);
        }
    }
}
