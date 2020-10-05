using System;
using Core.Events;

namespace Carts.Carts.Events
{
    public class CartConfirmed: IEvent
    {
        public Guid CartId { get; }

        public CartStatus CartStatus { get; }

        public DateTime ConfirmedAt { get; }

        public CartConfirmed(Guid cartId, CartStatus cartStatus, DateTime confirmedAt)
        {
            CartId = cartId;
            CartStatus = cartStatus;
            ConfirmedAt = confirmedAt;
        }
    }
}
