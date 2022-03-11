namespace Carts.ShoppingCarts.CancelingCart;

public record ShoppingCartCanceled(
    Guid CartId,
    DateTime CanceledAt
)
{
    public static ShoppingCartCanceled Create(Guid cartId, DateTime canceledAt)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (canceledAt == default)
            throw new ArgumentOutOfRangeException(nameof(canceledAt));

        return new ShoppingCartCanceled(cartId, canceledAt);
    }
}
