using System;
using Ardalis.GuardClauses;
using Carts.Carts.Products;
using Core.Events;

namespace Carts.Carts.RemovingProduct
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
