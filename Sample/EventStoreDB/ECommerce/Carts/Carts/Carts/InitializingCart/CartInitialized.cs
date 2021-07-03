using System;
using Ardalis.GuardClauses;
using Core.Events;

namespace Carts.Carts.InitializingCart
{
    public class CartInitialized: IEvent
    {
        public Guid CartId { get; }

        public Guid ClientId { get; }

        public CartStatus CartStatus { get; }

        public CartInitialized(Guid cartId, Guid clientId, CartStatus cartStatus)
        {
            CartId = cartId;
            ClientId = clientId;
            CartStatus = cartStatus;
        }

        public static CartInitialized Create(Guid cartId, Guid clientId, CartStatus cartStatus)
        {
            Guard.Against.Default(cartId, nameof(cartId));
            Guard.Against.Default(clientId, nameof(clientId));
            Guard.Against.Default(cartStatus, nameof(cartStatus));

            return new CartInitialized(cartId, clientId, cartStatus);
        }
    }
}
