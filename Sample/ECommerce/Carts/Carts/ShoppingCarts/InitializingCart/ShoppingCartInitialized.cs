using System;
using Core.Events;

namespace Carts.ShoppingCarts.InitializingCart;

public class ShoppingCartInitialized: IEvent
{
    public Guid CartId { get; }

    public Guid ClientId { get; }

    public ShoppingCartStatus ShoppingCartStatus { get; }

    public ShoppingCartInitialized(Guid cartId, Guid clientId, ShoppingCartStatus shoppingCartStatus)
    {
        CartId = cartId;
        ClientId = clientId;
        ShoppingCartStatus = shoppingCartStatus;
    }

    public static ShoppingCartInitialized Create(Guid cartId, Guid clientId, ShoppingCartStatus shoppingCartStatus)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (clientId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(clientId));
        if (shoppingCartStatus == default)
            throw new ArgumentOutOfRangeException(nameof(shoppingCartStatus));

        return new ShoppingCartInitialized(cartId, clientId, shoppingCartStatus);
    }
}
