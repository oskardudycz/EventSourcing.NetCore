using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Repositories;
using MediatR;

namespace Carts.Carts.ConfirmingCart
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

    internal class HandleConfirmCart:
        ICommandHandler<ConfirmCart>
    {
        private readonly IRepository<Cart> cartRepository;

        public HandleConfirmCart(
            IRepository<Cart> cartRepository
        )
        {
            this.cartRepository = cartRepository;
        }

        public Task<Unit> Handle(ConfirmCart command, CancellationToken cancellationToken)
        {
            return cartRepository.GetAndUpdate(
                command.CartId,
                cart => cart.Confirm(),
                cancellationToken);
        }
    }
}
