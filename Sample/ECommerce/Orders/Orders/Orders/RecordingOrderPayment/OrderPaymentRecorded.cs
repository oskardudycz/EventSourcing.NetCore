using System;
using System.Collections.Generic;
using Core.Events;
using Orders.Products;

namespace Orders.Orders.RecordingOrderPayment;

public class OrderPaymentRecorded: IEvent
{
    public Guid OrderId { get; }

    public Guid PaymentId { get; }

    public IReadOnlyList<PricedProductItem> ProductItems { get; }

    public decimal Amount { get; }

    public DateTime PaymentRecordedAt { get; }

    private OrderPaymentRecorded(
        Guid orderId,
        Guid paymentId,
        IReadOnlyList<PricedProductItem> productItems,
        decimal amount,
        DateTime paymentRecordedAt)
    {
        OrderId = orderId;
        PaymentId = paymentId;
        ProductItems = productItems;
        Amount = amount;
        PaymentRecordedAt = paymentRecordedAt;
    }

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