using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Repositories;
using MediatR;

namespace Carts.Carts.InitializingCart
{
    public class InitializeCart: ICommand
    {
        public Guid CartId { get; }

        public Guid ClientId { get; }

        private InitializeCart(Guid cartId, Guid clientId)
        {
            CartId = cartId;
            ClientId = clientId;
        }

        public static InitializeCart Create(Guid? cartId, Guid? clientId)
        {
            if (cartId == null || cartId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(cartId));
            if (clientId == null || clientId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(clientId));

            return new InitializeCart(cartId.Value, clientId.Value);
        }
    }

    internal class HandleInitializeCart:
        ICommandHandler<InitializeCart>
    {
        private readonly IRepository<Cart> cartRepository;

        public HandleInitializeCart(
            IRepository<Cart> cartRepository
        )
        {
            this.cartRepository = cartRepository;
        }

        public async Task<Unit> Handle(InitializeCart command, CancellationToken cancellationToken)
        {
            var cart = Cart.Initialize(command.CartId, command.ClientId);

            await cartRepository.Add(cart, cancellationToken);

            return Unit.Value;
        }
    }
}
