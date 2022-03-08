namespace Payments.Payments.CompletingPayment;

public record PaymentCompleted(
    Guid PaymentId,
    DateTime CompletedAt
)
{
    public static PaymentCompleted Create(Guid paymentId, DateTime completedAt)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        if (completedAt == default)
            throw new ArgumentOutOfRangeException(nameof(completedAt));

        return new PaymentCompleted(paymentId, completedAt);
    }
}
