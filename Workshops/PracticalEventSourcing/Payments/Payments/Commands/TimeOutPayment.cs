using System;
using Core.Commands;

namespace Payments.Payments.Commands
{
    public class TimeOutPayment: ICommand
    {
        public Guid PaymentId { get; }

        public DateTime TimedOutAt { get; }

        public TimeOutPayment(Guid orderId, Guid paymentId, DateTime timedOutAt)
        {
            PaymentId = paymentId;
            TimedOutAt = timedOutAt;
        }
    }
}
