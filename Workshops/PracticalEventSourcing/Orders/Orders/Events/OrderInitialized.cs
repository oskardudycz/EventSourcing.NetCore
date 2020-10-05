using System;
using System.Collections.Generic;
using Carts.Carts.ValueObjects;
using Core.Events;

namespace Orders.Orders.Events
{
    public class OrderInitialized: IEvent
    {
        public Guid OrderId { get; }
        public Guid ClientId { get; }

        public IReadOnlyList<PricedProductItem> ProductItems { get; }

        public decimal TotalPrice { get; }

        public DateTime InitializedAt { get; }

        public OrderInitialized(Guid orderId, Guid clientId, IReadOnlyList<PricedProductItem> productItems,
            decimal totalPrice, DateTime initializedAt)
        {
            OrderId = orderId;
            ClientId = clientId;
            ProductItems = productItems;
            TotalPrice = totalPrice;
            InitializedAt = initializedAt;
        }
    }
}
