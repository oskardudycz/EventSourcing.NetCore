using System;

namespace ECommerce.ShoppingCarts.Confirming;

public record ConfirmShoppingCart(
    Guid ShoppingCartId,
    uint Version
)
{
    public static ConfirmShoppingCart From(Guid? cartId, uint? version)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        if (version == null)
            throw new ArgumentOutOfRangeException(nameof(version));

        return new ConfirmShoppingCart(cartId.Value, version.Value);
    }

    public static ShoppingCartConfirmed Handle(ConfirmShoppingCart command, ShoppingCart shoppingCart)
    {
        if(shoppingCart.IsClosed)
            throw new InvalidOperationException($"Confirming cart in '{shoppingCart.Status}' status is not allowed.");

        return new ShoppingCartConfirmed(
            shoppingCart.Id,
            DateTime.UtcNow
        );
    }
}