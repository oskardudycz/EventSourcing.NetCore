using System;
using Carts.Carts.ValueObjects;
using Core.Events;

namespace Carts.Carts.Events
{
    public class ProductAdded: IEvent
    {
        public Guid CartId { get; }

        public PricedProductItem ProductItem { get; }

        public ProductAdded(Guid cartId, PricedProductItem productItem)
        {
            CartId = cartId;
            ProductItem = productItem;
        }
    }
}
