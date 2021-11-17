using System;
using Core.Events;

namespace Carts.Carts.ConfirmingCart;

public record CartConfirmed(
    Guid CartId,
    DateTime ConfirmedAt
): IEvent
{
    public static CartConfirmed Create(Guid cartId, DateTime confirmedAt)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (confirmedAt == default)
            throw new ArgumentOutOfRangeException(nameof(confirmedAt));

        return new CartConfirmed(cartId, confirmedAt);
    }
}