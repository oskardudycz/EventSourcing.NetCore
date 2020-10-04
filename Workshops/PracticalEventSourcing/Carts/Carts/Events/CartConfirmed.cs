using System;
using Core.Events;

namespace Carts.Carts.Events
{
    public class CartConfirmed: IEvent
    {
        public Guid CartId { get; }

        public CartStatus CartStatus { get; }

        public CartConfirmed(Guid cartId, CartStatus cartStatus)
        {
            CartId = cartId;
            CartStatus = cartStatus;
        }
    }
}
