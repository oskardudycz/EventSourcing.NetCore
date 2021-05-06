using System;
using Ardalis.GuardClauses;
using Core.Commands;

namespace Orders.Payments.Commands
{
    public class RequestPayment: ICommand
    {
        public Guid OrderId { get; }

        public decimal Amount { get; }

        private RequestPayment(Guid orderId, decimal amount)
        {
            OrderId = orderId;
            Amount = amount;
        }

        public static RequestPayment Create(Guid orderId, decimal amount)
        {
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.NegativeOrZero(amount, nameof(amount));

            return new RequestPayment(orderId, amount);
        }
    }
}
