using System;
using Core.Events;

namespace Carts.Carts.Events
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
    }
}
