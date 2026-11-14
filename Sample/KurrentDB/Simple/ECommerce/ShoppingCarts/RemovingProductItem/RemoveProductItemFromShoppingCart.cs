using ECommerce.ShoppingCarts.ProductItems;

namespace ECommerce.ShoppingCarts.RemovingProductItem;

public record RemoveProductItemFromShoppingCart(
    Guid ShoppingCartId,
    PricedProductItem ProductItem
)
{
    public static RemoveProductItemFromShoppingCart From(Guid? cartId, PricedProductItem? productItem)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (productItem == null)
            throw new ArgumentOutOfRangeException(nameof(productItem));

        return new RemoveProductItemFromShoppingCart(cartId.Value, productItem);
    }

    public static ProductItemRemovedFromShoppingCart Handle(
        RemoveProductItemFromShoppingCart command,
        ShoppingCart shoppingCart
    )
    {
        var (cartId, productItem) = command;

        if(shoppingCart.IsClosed)
            throw new InvalidOperationException($"Adding product item for cart in '{shoppingCart.Status}' status is not allowed.");

        shoppingCart.ProductItems.Remove(productItem);

        return new ProductItemRemovedFromShoppingCart(
            cartId,
            productItem
        );
    }
}
