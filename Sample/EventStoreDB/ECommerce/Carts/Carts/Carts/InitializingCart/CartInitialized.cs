using System;
using Core.Events;

namespace Carts.Carts.InitializingCart;

public class CartInitialized: IEvent
{
    public Guid CartId { get; }

    public Guid ClientId { get; }

    public ShoppingCartStatus ShoppingCartStatus { get; }

    public CartInitialized(Guid cartId, Guid clientId, ShoppingCartStatus shoppingCartStatus)
    {
        CartId = cartId;
        ClientId = clientId;
        ShoppingCartStatus = shoppingCartStatus;
    }

    public static CartInitialized Create(Guid cartId, Guid clientId, ShoppingCartStatus shoppingCartStatus)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (clientId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(clientId));
        if (shoppingCartStatus == default)
            throw new ArgumentOutOfRangeException(nameof(shoppingCartStatus));

        return new CartInitialized(cartId, clientId, shoppingCartStatus);
    }
}