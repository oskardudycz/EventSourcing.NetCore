using System;
using Ardalis.GuardClauses;
using Core.Events;

namespace Carts.Carts.Events
{
    public class CartConfirmed: IEvent
    {
        public Guid CartId { get; }

        public DateTime ConfirmedAt { get; }

        private CartConfirmed(Guid cartId, DateTime confirmedAt)
        {
            CartId = cartId;
            ConfirmedAt = confirmedAt;
        }

        public static CartConfirmed Create(Guid cartId, DateTime confirmedAt)
        {
            Guard.Against.Default(cartId, nameof(cartId));
            Guard.Against.Default(confirmedAt, nameof(confirmedAt));

            return new CartConfirmed(cartId, confirmedAt);
        }
    }
}
