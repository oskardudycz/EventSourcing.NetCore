using System;
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
            if (paymentId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(paymentId));
            if (discardedAt == default)
                throw new ArgumentOutOfRangeException(nameof(discardedAt));

            return new PaymentDiscarded(paymentId, discardReason, discardedAt);
        }
    }
}
