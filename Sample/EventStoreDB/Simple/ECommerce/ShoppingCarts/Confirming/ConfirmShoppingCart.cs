using System;

namespace ECommerce.ShoppingCarts.Confirming
{
    public record ConfirmShoppingCart(
        Guid ShoppingCartId
    )
    {
        public static ConfirmShoppingCart From(Guid? cartId)
        {
            if (cartId == null || cartId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(cartId));

            return new ConfirmShoppingCart(cartId.Value);
        }

        public static ShoppingCartConfirmed Handle(ConfirmShoppingCart command, ShoppingCart shoppingCart)
        {
            if(shoppingCart.Status != ShoppingCartStatus.Pending)
                throw new InvalidOperationException($"Confirming cart in '{shoppingCart.Status}' status is not allowed.");

            return new ShoppingCartConfirmed(
                shoppingCart.Id,
                DateTime.UtcNow
            );
        }
    }
}
