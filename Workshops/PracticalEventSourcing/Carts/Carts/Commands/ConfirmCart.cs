using System;
using Core.Commands;

namespace Carts.Carts.Commands
{
    public class ConfirmCart: ICommand
    {
        public Guid CartId { get; }

        public ConfirmCart(Guid cartId)
        {
            CartId = cartId;
        }
    }
}
