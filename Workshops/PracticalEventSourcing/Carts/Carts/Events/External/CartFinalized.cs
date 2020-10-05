using System;
using System.Collections.Generic;
using Carts.Carts.ValueObjects;
using Core.Events;

namespace Carts.Carts.Events.External
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
