using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Core.Commands;
using ECommerce.Core.Entities;

namespace ECommerce.ShoppingCarts.Initializing
{
    public record InitializeShoppingCart(
        Guid ShoppingCartId,
        Guid ClientId
    )
    {
        public static InitializeShoppingCart From(Guid? cartId, Guid? clientId)
        {
            if (cartId == null || cartId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(cartId));
            if (clientId == null || clientId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(clientId));

            return new InitializeShoppingCart(cartId.Value, clientId.Value);
        }
    }

    public class HandleInitializeShoppingCart : ICommandHandler<InitializeShoppingCart>
    {
        private readonly IEventStoreDBRepository<ShoppingCart> repository;

        public HandleInitializeShoppingCart(IEventStoreDBRepository<ShoppingCart> repository)
        {
            this.repository = repository;
        }

        public async ValueTask Handle(InitializeShoppingCart command, CancellationToken ct)
        {
            var @event = Handle(command);

            await repository.Append(command.ShoppingCartId, @event, ct);
        }

        public static ShoppingCartInitialized Handle(InitializeShoppingCart command)
        {
            var (shoppingCartId, clientId) = command;

            return new ShoppingCartInitialized(
                shoppingCartId,
                clientId,
                ShoppingCartStatus.Pending
            );
        }
    }
}
