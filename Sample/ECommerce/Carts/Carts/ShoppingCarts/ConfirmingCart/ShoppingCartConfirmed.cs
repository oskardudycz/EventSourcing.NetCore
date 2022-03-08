namespace Carts.ShoppingCarts.ConfirmingCart;

public record ShoppingCartConfirmed(
    Guid CartId,
    DateTime ConfirmedAt
)
{
    public static ShoppingCartConfirmed Create(Guid cartId, DateTime confirmedAt)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (confirmedAt == default)
            throw new ArgumentOutOfRangeException(nameof(confirmedAt));

        return new ShoppingCartConfirmed(cartId, confirmedAt);
    }
}
