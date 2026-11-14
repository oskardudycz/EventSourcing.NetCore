namespace Carts.ShoppingCarts.OpeningCart;

public record ShoppingCartOpened(
    Guid CartId,
    Guid ClientId,
    ShoppingCartStatus ShoppingCartStatus
)
{
    public static ShoppingCartOpened Create(Guid cartId, Guid clientId, ShoppingCartStatus shoppingCartStatus)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (clientId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(clientId));
        if (shoppingCartStatus == default)
            throw new ArgumentOutOfRangeException(nameof(shoppingCartStatus));

        return new ShoppingCartOpened(cartId, clientId, shoppingCartStatus);
    }
}
