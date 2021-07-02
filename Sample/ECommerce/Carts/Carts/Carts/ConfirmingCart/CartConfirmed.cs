using System;
using Ardalis.GuardClauses;
using Core.Events;

namespace Carts.Carts.ConfirmingCart
{
    public record CartConfirmed(
        Guid CartId,
        DateTime ConfirmedAt
    ): IEvent
    {
        public static CartConfirmed Create(Guid cartId, DateTime confirmedAt)
        {
            Guard.Against.Default(cartId, nameof(cartId));
            Guard.Against.Default(confirmedAt, nameof(confirmedAt));

            return new CartConfirmed(cartId, confirmedAt);
        }
    }
}
