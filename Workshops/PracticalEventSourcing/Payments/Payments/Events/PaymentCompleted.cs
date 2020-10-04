using System;
using Core.Events;
using Payments.Payments.Events.Enums;

namespace Payments.Payments.Events
{
    public class PaymentCompleted: IEvent
    {
        public Guid PaymentId { get; }

        public PaymentCompleted(Guid paymentId)
        {
            PaymentId = paymentId;
        }
    }
}
