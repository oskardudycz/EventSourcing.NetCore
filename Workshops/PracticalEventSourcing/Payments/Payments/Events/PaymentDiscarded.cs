using System;
using Core.Events;
using Payments.Payments.Events.Enums;

namespace Payments.Payments.Events
{
    public class PaymentDiscarded: IEvent
    {
        public Guid PaymentId { get; }
        public DiscardReason DiscardReason { get; }

        public PaymentStatus PaymentStatus { get; }

        public PaymentDiscarded(Guid paymentId, DiscardReason discardReason, PaymentStatus paymentStatus)
        {
            PaymentId = paymentId;
            DiscardReason = discardReason;
            PaymentStatus = paymentStatus;
        }
    }
}
