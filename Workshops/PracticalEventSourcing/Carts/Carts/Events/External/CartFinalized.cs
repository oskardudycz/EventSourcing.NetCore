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

        public IReadOnlyList<ProductItem> ProductItems { get; }

        public Guid TotalPrice { get; }

        public CartFinalized(Guid cartId, Guid clientId, IReadOnlyList<ProductItem> productItems, Guid totalPrice)
        {
            CartId = cartId;
            ClientId = clientId;
            ProductItems = productItems;
            TotalPrice = totalPrice;
        }
    }
}
