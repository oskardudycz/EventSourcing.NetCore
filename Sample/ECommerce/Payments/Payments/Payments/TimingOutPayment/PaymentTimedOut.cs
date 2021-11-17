using System;
using Core.Events;

namespace Payments.Payments.TimingOutPayment;

public class PaymentTimedOut: IEvent
{
    public Guid PaymentId { get; }

    public DateTime TimedOutAt { get; }

    private PaymentTimedOut(Guid paymentId, DateTime timedOutAt)
    {
        PaymentId = paymentId;
        TimedOutAt = timedOutAt;
    }

    public static PaymentTimedOut Create(Guid paymentId, in DateTime timedOutAt)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        if (timedOutAt == default)
            throw new ArgumentOutOfRangeException(nameof(timedOutAt));

        return new PaymentTimedOut(paymentId, timedOutAt);
    }
}