using System;
using Ardalis.GuardClauses;
using Carts.Carts.ValueObjects;
using Core.Events;

namespace Carts.Carts.AddingProduct
{
    public class ProductAdded: IEvent
    {
        public Guid CartId { get; }

        public PricedProductItem ProductItem { get; }

        private ProductAdded(Guid cartId, PricedProductItem productItem)
        {
            CartId = cartId;
            ProductItem = productItem;
        }

        public static ProductAdded Create(Guid cartId, PricedProductItem productItem)
        {
            Guard.Against.Default(cartId, nameof(cartId));
            Guard.Against.Null(productItem, nameof(productItem));

            return new ProductAdded(cartId, productItem);
        }
    }
}
