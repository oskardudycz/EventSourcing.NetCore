using System;
using Ardalis.GuardClauses;
using Core.Commands;
using Orders.Payments.Enums;

namespace Orders.Payments.Commands
{
    public class DiscardPayment: ICommand
    {
        public Guid PaymentId { get; }

        public DiscardReason DiscardReason { get; }

        private DiscardPayment(Guid paymentId, DiscardReason discardReason)
        {
            PaymentId = paymentId;
            DiscardReason = discardReason;
        }

        public static DiscardPayment Create(Guid paymentId)
        {
            Guard.Against.Default(paymentId, nameof(paymentId));

            return new DiscardPayment(paymentId, DiscardReason.OrderCancelled);
        }
    }
}
