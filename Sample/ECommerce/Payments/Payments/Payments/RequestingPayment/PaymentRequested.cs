using System;
using Ardalis.GuardClauses;
using Core.Events;

namespace Payments.Payments.RequestingPayment
{
    public class PaymentRequested: IEvent
    {
        public Guid PaymentId { get; }
        public Guid OrderId { get; }
        public decimal Amount { get; }

        private PaymentRequested(Guid paymentId, Guid orderId, decimal amount)
        {
            PaymentId = paymentId;
            OrderId = orderId;
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
