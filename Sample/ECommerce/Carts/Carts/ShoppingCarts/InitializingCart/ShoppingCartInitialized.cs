namespace Carts.ShoppingCarts.InitializingCart;

public record ShoppingCartInitialized(
    Guid CartId,
    Guid ClientId,
    ShoppingCartStatus ShoppingCartStatus
)
{
    public static ShoppingCartInitialized Create(Guid cartId, Guid clientId, ShoppingCartStatus shoppingCartStatus)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (clientId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(clientId));
        if (shoppingCartStatus == default)
            throw new ArgumentOutOfRangeException(nameof(shoppingCartStatus));

        return new ShoppingCartInitialized(cartId, clientId, shoppingCartStatus);
    }
}
