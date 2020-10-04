using System;
using Core.Events;
using Payments.Payments.Events.Enums;

namespace Payments.Payments.Events
{
    public class PaymentCompleted: IEvent
    {
        public Guid PaymentId { get; }

        public PaymentStatus PaymentStatus { get; }

        public PaymentCompleted(Guid paymentId, PaymentStatus paymentStatus)
        {
            PaymentId = paymentId;
            PaymentStatus = paymentStatus;
        }
    }
}
