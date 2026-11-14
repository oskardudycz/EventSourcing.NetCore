using Carts.ShoppingCarts.Products;

namespace Carts.ShoppingCarts.RemovingProduct;

public record ProductRemoved(
    Guid CartId,
    PricedProductItem ProductItem
)
{
    public static ProductRemoved Create(Guid cartId, PricedProductItem productItem)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new ProductRemoved(cartId, productItem);
    }
}
