using System;
using Core.Events;
using Payments.Payments.Events.Enums;

namespace Payments.Payments.Events
{
    public class PaymentDiscarded: IEvent
    {
        public Guid PaymentId { get; }
        public DiscardReason DiscardReason { get; }

        public PaymentDiscarded(Guid paymentId, DiscardReason discardReason)
        {
            PaymentId = paymentId;
            DiscardReason = discardReason;
        }
    }
}
