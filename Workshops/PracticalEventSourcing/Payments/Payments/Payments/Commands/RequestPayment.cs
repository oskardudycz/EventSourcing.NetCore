using System;
using Ardalis.GuardClauses;
using Core.Commands;

namespace Payments.Payments.Commands
{
    public class RequestPayment: ICommand
    {
        public Guid PaymentId { get; }

        public Guid OrderId { get; }

        public decimal Amount { get; }

        private RequestPayment(
            Guid paymentId,
            Guid orderId,
            decimal amount
        )
        {
            PaymentId = paymentId;
            OrderId = orderId;
            Amount = amount;
        }
        public static RequestPayment Create(
            Guid paymentId,
            Guid orderId,
            decimal amount
        )
        {
            Guard.Against.Default(paymentId, nameof(paymentId));
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.NegativeOrZero(amount, nameof(amount));

            return new RequestPayment(paymentId, orderId, amount);
        }
    }
}
