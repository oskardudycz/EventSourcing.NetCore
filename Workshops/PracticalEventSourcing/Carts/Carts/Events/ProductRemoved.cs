using System;
using Carts.Carts.ValueObjects;
using Core.Events;

namespace Carts.Carts.Events
{
    public class ProductRemoved: IEvent
    {
        public Guid CartId { get; }

        public ProductItem ProductItem { get; }

        public ProductRemoved(Guid cartId, ProductItem productItem)
        {
            CartId = cartId;
            ProductItem = productItem;
        }
    }
}
