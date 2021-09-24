using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Core.Commands;
using ECommerce.Core.Entities;

namespace ECommerce.ShoppingCarts.ConfirmingCart
{
    public record ConfirmCart(
        Guid ShoppingCartId
    )
    {
        public static ConfirmCart From(Guid? cartId)
        {
            if (cartId == null || cartId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(cartId));

            return new ConfirmCart(cartId.Value);
        }
    }

    internal class HandleConfirmCart: ICommandHandler<ConfirmCart>
    {
        private readonly IEventStoreDBRepository<ShoppingCart> repository;

        public HandleConfirmCart(IEventStoreDBRepository<ShoppingCart> repository)
        {
            this.repository = repository;
        }

        public async ValueTask Handle(ConfirmCart command, CancellationToken ct)
        {
            var shoppingCart = await repository.Find(
                command.ShoppingCartId,
                ShoppingCart.When,
                ct
            );

            var @event = Handle(shoppingCart, command);

            await repository.Append(command.ShoppingCartId, @event, ct);
        }

        public static ShoppingCartConfirmed Handle(ShoppingCart shoppingCart, ConfirmCart command)
        {
            if(shoppingCart.Status != ShoppingCartStatus.Pending)
                throw new InvalidOperationException($"Confirming cart in '{shoppingCart.Status}' status is not allowed.");

            return new ShoppingCartConfirmed(
                shoppingCart.Id,
                DateTime.UtcNow
            );
        }
    }
}
