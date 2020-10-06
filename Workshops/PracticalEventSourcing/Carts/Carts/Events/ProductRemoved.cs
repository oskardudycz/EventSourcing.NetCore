using System;
using Ardalis.GuardClauses;
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

        public static ProductRemoved Create(Guid cartId, PricedProductItem productItem)
        {
            Guard.Against.Default(cartId, nameof(cartId));
            Guard.Against.Null(productItem, nameof(productItem));

            return new ProductRemoved(cartId, productItem);
        }
    }
}
