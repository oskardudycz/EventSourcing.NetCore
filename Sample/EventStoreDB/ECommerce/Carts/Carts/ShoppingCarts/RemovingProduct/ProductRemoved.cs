using System;
using Carts.ShoppingCarts.Products;
using Core.Events;

namespace Carts.ShoppingCarts.RemovingProduct;

public record ProductRemoved(
    Guid CartId,
    PricedProductItem ProductItem
): IEvent
{
    public static ProductRemoved Create(Guid cartId, PricedProductItem productItem)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new ProductRemoved(cartId, productItem);
    }
}
