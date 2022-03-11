namespace Carts.ShoppingCarts.OpeningCart;

public record ShoppingCartOpened(
    Guid CartId,
    Guid ClientId
)
{
    public static ShoppingCartOpened Create(Guid cartId, Guid clientId)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (clientId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(clientId));

        return new ShoppingCartOpened(cartId, clientId);
    }
}
