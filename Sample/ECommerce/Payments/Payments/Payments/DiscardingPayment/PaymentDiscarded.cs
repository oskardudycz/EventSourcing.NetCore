namespace Payments.Payments.DiscardingPayment;

public record PaymentDiscarded(
    Guid PaymentId,
    DiscardReason DiscardReason,
    DateTime DiscardedAt)
{
    public static PaymentDiscarded Create(Guid paymentId, DiscardReason discardReason, DateTime discardedAt)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        if (discardedAt == default)
            throw new ArgumentOutOfRangeException(nameof(discardedAt));

        return new PaymentDiscarded(paymentId, discardReason, discardedAt);
    }
}
