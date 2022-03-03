namespace ECommerce.ShoppingCarts.Initializing;

public record InitializeShoppingCart(
    Guid ShoppingCartId,
    Guid ClientId
)
{
    public static InitializeShoppingCart From(Guid? cartId, Guid? clientId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (clientId == null || clientId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(clientId));

        return new InitializeShoppingCart(cartId.Value, clientId.Value);
    }

    public static ShoppingCartInitialized Handle(InitializeShoppingCart command)
    {
        var (shoppingCartId, clientId) = command;

        return new ShoppingCartInitialized(
            shoppingCartId,
            clientId
        );
    }
}