using System;
using Core.Events;
using Payments.Payments.Events.Enums;

namespace Payments.Payments.Events
{
    public class PaymentTimedOut: IEvent
    {
        public Guid PaymentId { get; }

        public DateTime TimedOutAt { get; }

        public PaymentStatus PaymentStatus { get; }

        public PaymentTimedOut(Guid orderId, Guid paymentId, DateTime timedOutAt, PaymentStatus paymentStatus)
        {
            PaymentId = paymentId;
            TimedOutAt = timedOutAt;
            PaymentStatus = paymentStatus;
        }

    }
}
