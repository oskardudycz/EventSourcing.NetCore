namespace Payments.Payments.TimingOutPayment;

public record PaymentTimedOut(
    Guid PaymentId,
    DateTime TimedOutAt
)
{
    public static PaymentTimedOut Create(Guid paymentId, in DateTime timedOutAt)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        if (timedOutAt == default)
            throw new ArgumentOutOfRangeException(nameof(timedOutAt));

        return new PaymentTimedOut(paymentId, timedOutAt);
    }
}
