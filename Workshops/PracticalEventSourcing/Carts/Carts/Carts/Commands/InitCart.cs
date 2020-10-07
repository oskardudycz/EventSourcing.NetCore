using System;
using Core.Commands;

namespace Carts.Carts.Commands
{
    public class InitCart: ICommand
    {
        public Guid CartId { get; }

        public Guid ClientId { get; }

        public InitCart(Guid cartId, Guid clientId)
        {
            CartId = cartId;
            ClientId = clientId;
        }
    }
}
