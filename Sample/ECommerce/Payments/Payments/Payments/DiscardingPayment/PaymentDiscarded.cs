using System;
using Ardalis.GuardClauses;
using Core.Events;

namespace Payments.Payments.DiscardingPayment
{
    public class PaymentDiscarded: IEvent
    {
        public Guid PaymentId { get; }
        public DiscardReason DiscardReason { get; }

        public DateTime DiscardedAt { get; }

        private PaymentDiscarded(Guid paymentId, DiscardReason discardReason, DateTime discardedAt)
        {
            PaymentId = paymentId;
            DiscardReason = discardReason;
            DiscardedAt = discardedAt;
        }

        public static PaymentDiscarded Create(Guid paymentId, DiscardReason discardReason, DateTime discardedAt)
        {
            Guard.Against.Default(paymentId, nameof(paymentId));
            Guard.Against.Default(discardedAt, nameof(discardedAt));

            return new PaymentDiscarded(paymentId, discardReason, discardedAt);
        }
    }
}
