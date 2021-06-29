using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using Core.Events;
using Orders.Products.ValueObjects;

namespace Orders.Orders.Events
{
    public class OrderInitialized: IEvent
    {
        public Guid OrderId { get; }
        public Guid ClientId { get; }

        public IReadOnlyList<PricedProductItem> ProductItems { get; }

        public decimal TotalPrice { get; }

        public DateTime InitializedAt { get; }

        private OrderInitialized(
            Guid orderId,
            Guid clientId,
            IReadOnlyList<PricedProductItem> productItems,
            decimal totalPrice,
            DateTime initializedAt)
        {
            OrderId = orderId;
            ClientId = clientId;
            ProductItems = productItems;
            TotalPrice = totalPrice;
            InitializedAt = initializedAt;
        }

        public static OrderInitialized Create(
            Guid orderId,
            Guid clientId,
            IReadOnlyList<PricedProductItem> productItems,
            decimal totalPrice,
            DateTime initializedAt)
        {
            Guard.Against.Default(orderId, nameof(orderId));
            Guard.Against.Default(clientId, nameof(clientId));
            Guard.Against.NullOrEmpty(productItems, nameof(productItems));
            Guard.Against.NegativeOrZero(totalPrice, nameof(totalPrice));
            Guard.Against.Default(initializedAt, nameof(initializedAt));

            return new OrderInitialized(orderId, clientId, productItems, totalPrice, initializedAt);
        }
    }
}
