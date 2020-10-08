using System;
using Ardalis.GuardClauses;
using Core.Commands;

namespace Payments.Payments.Commands
{
    public class TimeOutPayment: ICommand
    {
        public Guid PaymentId { get; }

        public DateTime TimedOutAt { get; }

        private TimeOutPayment(Guid paymentId, DateTime timedOutAt)
        {
            PaymentId = paymentId;
            TimedOutAt = timedOutAt;
        }

        public static TimeOutPayment Create(Guid paymentId, DateTime timedOutAt)
        {
            Guard.Against.Default(paymentId, nameof(paymentId));
            Guard.Against.Default(timedOutAt, nameof(timedOutAt));

            return new TimeOutPayment(paymentId, timedOutAt);
        }
    }
}
