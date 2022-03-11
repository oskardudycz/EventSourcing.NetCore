namespace ECommerce.ShoppingCarts.Canceling;

public record CancelShoppingCart(
    Guid ShoppingCartId
)
{
    public static CancelShoppingCart From(Guid? cartId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new CancelShoppingCart(cartId.Value);
    }

    public static ShoppingCartCanceled Handle(CancelShoppingCart command, ShoppingCart shoppingCart)
    {
        if(shoppingCart.IsClosed)
            throw new InvalidOperationException($"Confirming cart in '{shoppingCart.Status}' status is not allowed.");

        return new ShoppingCartCanceled(
            shoppingCart.Id,
            DateTime.UtcNow
        );
    }
}
