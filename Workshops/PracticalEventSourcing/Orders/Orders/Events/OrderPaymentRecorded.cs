using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using Carts.Carts.ValueObjects;
using Core.Events;

namespace Orders.Orders.Events
{
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
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.Default(paymentId, nameof(paymentId));
            Guard.Against.NullOrEmpty(productItems, nameof(productItems));
            Guard.Against.NegativeOrZero(amount, nameof(amount));
            Guard.Against.Default(recordedAt, nameof(recordedAt));

            return new OrderPaymentRecorded(
                orderId,
                paymentId,
                productItems,
                amount,
                recordedAt
            );
        }
    }
}
