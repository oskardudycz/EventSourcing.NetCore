using System;
using Core.Events;

namespace Carts.Carts.Events
{
    public class CartInitialized: IEvent
    {
        public Guid CartId { get; }

        public Guid ClientId { get; }

        public Guid CartStatus { get; }

        public CartInitialized(Guid cartId, Guid clientId, Guid cartStatus)
        {
            CartId = cartId;
            ClientId = clientId;
            CartStatus = cartStatus;
        }
    }
}
