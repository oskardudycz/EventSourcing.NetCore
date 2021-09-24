using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        DateTime? ConfirmedAt
        //IList<PricedProductItem> ProductItems
    )
    {
        private ShoppingCart(): this(default, default, default, default) { }

        public static ShoppingCart When(ShoppingCart entity, object @event)
        {
            return @event switch
            {
                ShoppingCartInitialized(var cartId, var clientId, var cartStatus) => entity with
                {
                    Id = cartId,
                    ClientId = clientId,
                    Status = cartStatus
                },
                ShoppingCartConfirmed (_, var confirmedAt) => entity with
                {
                    Status = ShoppingCartStatus.Confirmed, ConfirmedAt = confirmedAt
                },
                _ => entity
            };
        }
    }
}
