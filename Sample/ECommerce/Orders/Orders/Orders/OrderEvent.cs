using Core.Validation;
using Orders.Orders.CancellingOrder;
using Orders.Products;

namespace Orders.Orders;

public abstract record OrderEvent
{
    public record OrderInitiated(
        Guid OrderId,
        Guid ClientId,
        IReadOnlyList<PricedProductItem> ProductItems,
        decimal TotalPrice,
        DateTimeOffset InitiatedAt,
        DateTimeOffset TimeoutAfter
    ): OrderEvent
    {
        public static OrderInitiated From(
            Guid orderId,
            Guid clientId,
            IReadOnlyList<PricedProductItem> productItems,
            decimal totalPrice,
            DateTimeOffset initializedAt,
            DateTimeOffset timeoutAfter) =>
            new(
                orderId.NotEmpty(),
                clientId.NotEmpty(),
                productItems.Has(p => p.Count.Positive()),
                totalPrice.Positive(),
                initializedAt.NotEmpty(),
                timeoutAfter.NotEmpty()
            );
    }


    public record OrderPaymentRecorded(
        Guid OrderId,
        Guid PaymentId,
        IReadOnlyList<PricedProductItem> ProductItems,
        decimal Amount,
        DateTimeOffset PaymentRecordedAt
    ): OrderEvent
    {
        public static OrderPaymentRecorded Create(
            Guid orderId,
            Guid paymentId,
            IReadOnlyList<PricedProductItem> productItems,
            decimal amount,
            DateTimeOffset recordedAt
        ) =>
            new(
                orderId.NotEmpty(),
                paymentId.NotEmpty(),
                productItems.Has(p => p.Count.Positive()),
                amount.Positive(),
                recordedAt
            );
    }

    public record OrderCompleted(
        Guid OrderId,
        DateTimeOffset CompletedAt
    ): OrderEvent
    {
        public static OrderCompleted Create(Guid orderId, DateTimeOffset completedAt) =>
            new(orderId.NotEmpty(), completedAt.NotEmpty());
    }

    public record OrderCancelled(
        Guid OrderId,
        Guid? PaymentId,
        OrderCancellationReason OrderCancellationReason,
        DateTimeOffset CancelledAt
    ): OrderEvent
    {
        public static OrderCancelled Create(
            Guid orderId,
            Guid? paymentId,
            OrderCancellationReason orderCancellationReason,
            DateTimeOffset cancelledAt
        ) =>
            new(orderId.NotEmpty(), paymentId.NotEmpty(), orderCancellationReason.NotEmpty(), cancelledAt.NotEmpty());
    }

    // QUESTION TO THE READER: How to split OrderCancelled into OrderCancelled and OrderTimedOut

    private OrderEvent() { }
}
