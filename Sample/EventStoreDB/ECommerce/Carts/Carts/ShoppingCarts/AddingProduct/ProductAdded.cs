using System;
using Carts.ShoppingCarts.Products;
using Core.Events;

namespace Carts.ShoppingCarts.AddingProduct;

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
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new ProductAdded(cartId, productItem);
    }
}