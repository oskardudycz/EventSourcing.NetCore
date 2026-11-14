namespace Carts.ShoppingCarts.CancelingCart;

public record ShoppingCartCanceled(
    Guid CartId,
    DateTime CanceledAt
)
{
    public static ShoppingCartCanceled Create(Guid cartId, DateTime canceledAt)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentNullException(nameof(cartId));
        if (canceledAt == default)
            throw new ArgumentNullException(nameof(canceledAt));

        return new ShoppingCartCanceled(cartId, canceledAt);
    }
}
