using Carts.ShoppingCarts.Products;

namespace Carts.ShoppingCarts.AddingProduct;

public record ProductAdded(
    Guid CartId,
    PricedProductItem ProductItem
)
{
    public static ProductAdded Create(Guid cartId, PricedProductItem productItem)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new ProductAdded(cartId, productItem);
    }
}
