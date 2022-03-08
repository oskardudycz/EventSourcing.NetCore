namespace Payments.Payments.RequestingPayment;

public record PaymentRequested(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount
)
{
    public static PaymentRequested Create(Guid paymentId, Guid orderId, in decimal amount)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        if (orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId));
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        return new PaymentRequested(paymentId, orderId, amount);
    }
}
