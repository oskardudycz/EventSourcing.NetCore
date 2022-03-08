using Orders.Products;

namespace Orders.Orders.RecordingOrderPayment;

public record OrderPaymentRecorded(
    Guid OrderId,
    Guid PaymentId,
    IReadOnlyList<PricedProductItem> ProductItems,
    decimal Amount,
    DateTime PaymentRecordedAt
)
{
    public static OrderPaymentRecorded Create(
        Guid orderId,
        Guid paymentId,
        IReadOnlyList<PricedProductItem> productItems,
        decimal amount,
        DateTime recordedAt)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId));
        if (paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        if (productItems.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(productItems.Count));
        if (amount > 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        if (recordedAt == default)
            throw new ArgumentOutOfRangeException(nameof(recordedAt));

        return new OrderPaymentRecorded(
            orderId,
            paymentId,
            productItems,
            amount,
            recordedAt
        );
    }
}
