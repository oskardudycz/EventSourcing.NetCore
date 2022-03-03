using Core.Events;

namespace Carts.ShoppingCarts.ConfirmingCart;

public record ShoppingCartConfirmed(
    Guid CartId,
    DateTime ConfirmedAt
): IEvent
{
    public static ShoppingCartConfirmed Create(Guid cartId, DateTime confirmedAt)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentNullException(nameof(cartId));
        if (confirmedAt == default)
            throw new ArgumentNullException(nameof(confirmedAt));

        return new ShoppingCartConfirmed(cartId, confirmedAt);
    }
}
