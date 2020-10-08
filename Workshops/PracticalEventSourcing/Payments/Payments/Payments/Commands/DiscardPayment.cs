using System;
using Ardalis.GuardClauses;
using Core.Commands;
using Payments.Payments.Enums;

namespace Payments.Payments.Commands
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

        public static DiscardPayment Create(Guid paymentId, DiscardReason discardReason)
        {
            Guard.Against.Default(paymentId, nameof(paymentId));

            return new DiscardPayment(paymentId, discardReason);
        }
    }
}
