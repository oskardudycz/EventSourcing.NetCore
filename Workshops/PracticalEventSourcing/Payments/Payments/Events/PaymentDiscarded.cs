using System;
using Core.Events;
using Payments.Payments.Events.Enums;

namespace Payments.Payments.Events
{
    public class PaymentDiscarded: IEvent
    {
        public Guid PaymentId { get; }
        public DiscardReason DiscardReason { get; }

        public DateTime DiscardedAt { get; }

        public PaymentDiscarded(Guid paymentId, DiscardReason discardReason, DateTime discardedAt)
        {
            PaymentId = paymentId;
            DiscardReason = discardReason;
            DiscardedAt = discardedAt;
        }
    }
}
