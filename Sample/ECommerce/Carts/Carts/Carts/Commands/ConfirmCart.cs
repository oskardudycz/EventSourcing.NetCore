using System;
using Ardalis.GuardClauses;
using Core.Commands;

namespace Carts.Carts.Commands
{
    public class ConfirmCart: ICommand
    {
        public Guid CartId { get; }

        private ConfirmCart(Guid cartId)
        {
            CartId = cartId;
        }

        public static ConfirmCart Create(Guid cartId)
        {
            Guard.Against.Default(cartId, nameof(cartId));

            return new ConfirmCart(cartId);
        }
    }
}
