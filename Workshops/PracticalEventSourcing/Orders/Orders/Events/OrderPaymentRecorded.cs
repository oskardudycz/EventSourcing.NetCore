using System;
using System.Collections.Generic;
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

        public OrderPaymentRecorded(Guid orderId, Guid paymentId, IReadOnlyList<PricedProductItem> productItems,
            decimal amount, DateTime paymentRecordedAt)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            ProductItems = productItems;
            Amount = amount;
            PaymentRecordedAt = paymentRecordedAt;
        }
    }
}
