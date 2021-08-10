using System;
using Core.Events;

namespace Payments.Payments.CompletingPayment
{
    public class PaymentCompleted: IEvent
    {
        public Guid PaymentId { get; }

        public DateTime CompletedAt { get; }

        private PaymentCompleted(Guid paymentId, DateTime completedAt)
        {
            PaymentId = paymentId;
            CompletedAt = completedAt;
        }

        public static PaymentCompleted Create(Guid paymentId, DateTime completedAt)
        {
            if (paymentId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(paymentId));
            if (completedAt == default)
                throw new ArgumentOutOfRangeException(nameof(completedAt));

            return new PaymentCompleted(paymentId, completedAt);
        }
    }
}
