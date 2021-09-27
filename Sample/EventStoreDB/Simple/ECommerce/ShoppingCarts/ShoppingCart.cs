using System;

namespace ECommerce.ShoppingCarts
{
    public enum ShoppingCartStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 3
    }

    public record ShoppingCartInitialized(
        Guid ShoppingCartId,
        Guid ClientId,
        ShoppingCartStatus ShoppingCartStatus
    );

    public record ShoppingCartConfirmed(
        Guid CartId,
        DateTime ConfirmedAt
    );

    public record ShoppingCart(
        Guid Id,
        Guid ClientId,
        ShoppingCartStatus Status,
        DateTime? ConfirmedAt = null
    )
    {
        public static ShoppingCart When(ShoppingCart? entity, object @event)
        {
            return @event switch
            {
                ShoppingCartInitialized (var cartId, var clientId, var cartStatus) =>
                    new ShoppingCart(cartId, clientId, cartStatus),

                ShoppingCartConfirmed (_, var confirmedAt) =>
                    entity! with
                    {
                        Status = ShoppingCartStatus.Confirmed,
                        ConfirmedAt = confirmedAt
                    },
                _ => entity!
            };
        }

        public static string MapToStreamId(Guid shoppingCartId)
            => $"shopping_cart-{shoppingCartId}";
    }
}
