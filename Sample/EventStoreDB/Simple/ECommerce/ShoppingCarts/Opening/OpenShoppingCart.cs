namespace ECommerce.ShoppingCarts.Opening;

public record OpenShoppingCart(
    Guid ShoppingCartId,
    Guid ClientId
)
{
    public static OpenShoppingCart From(Guid? cartId, Guid? clientId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (clientId == null || clientId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(clientId));

        return new OpenShoppingCart(cartId.Value, clientId.Value);
    }

    public static ShoppingCartOpened Handle(OpenShoppingCart command)
    {
        var (shoppingCartId, clientId) = command;

        return new ShoppingCartOpened(
            shoppingCartId,
            clientId
        );
    }
}
