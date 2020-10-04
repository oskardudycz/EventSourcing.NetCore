using System;
using Carts.Carts.ValueObjects;
using Core.Events;

namespace Carts.Carts.Events
{
    public class ProductRemoved: IEvent
    {
        public Guid CartId { get; }

        public PricedProductItem ProductItem { get; }

        public ProductRemoved(Guid cartId, PricedProductItem productItem)
        {
            CartId = cartId;
            ProductItem = productItem;
        }
    }
}
