using System;
using System.Collections.Generic;
using Core.Events;
using Orders.Products;
using Orders.Products.ValueObjects;

namespace Orders.Carts.Events
{
    public class CartFinalized: IExternalEvent
    {
        public Guid CartId { get; }

        public Guid ClientId { get; }

        public IReadOnlyList<PricedProductItem> ProductItems { get; }

        public decimal TotalPrice { get; }

        public DateTime FinalizedAt { get; }

        public CartFinalized(
            Guid cartId,
            Guid clientId,
            IReadOnlyList<PricedProductItem> productItems,
            decimal totalPrice,
            DateTime finalizedAt)
        {
            CartId = cartId;
            ClientId = clientId;
            ProductItems = productItems;
            TotalPrice = totalPrice;
            FinalizedAt = finalizedAt;
        }
    }
}
