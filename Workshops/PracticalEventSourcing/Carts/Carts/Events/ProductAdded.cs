using System;
using Carts.Carts.ValueObjects;
using Core.Events;

namespace Carts.Carts.Events
{
    public class ProductAdded: IEvent
    {
        public Guid CartId { get; }

        public ProductItem ProductItem { get; }

        public ProductAdded(Guid cartId, ProductItem productItem)
        {
            CartId = cartId;
            ProductItem = productItem;
        }
    }
}
