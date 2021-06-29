using System;
using Ardalis.GuardClauses;
using Core.Commands;

namespace Carts.Carts.Commands
{
    public class InitCart: ICommand
    {
        public Guid CartId { get; }

        public Guid ClientId { get; }

        private InitCart(Guid cartId, Guid clientId)
        {
            CartId = cartId;
            ClientId = clientId;
        }

        public static InitCart Create(Guid cartId, Guid clientId)
        {
            Guard.Against.Default(cartId, nameof(cartId));
            Guard.Against.Default(clientId, nameof(clientId));

            return new InitCart(cartId, clientId);
        }
    }
}
