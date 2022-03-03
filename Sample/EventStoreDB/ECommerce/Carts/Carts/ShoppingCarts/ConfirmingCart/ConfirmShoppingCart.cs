using Core.Commands;
using Core.EventStoreDB.OptimisticConcurrency;
using Core.EventStoreDB.Repository;
using MediatR;

namespace Carts.ShoppingCarts.ConfirmingCart;

public record ConfirmShoppingCart(
    Guid CartId
): ICommand
{
    public static ConfirmShoppingCart Create(Guid? cartId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new ConfirmShoppingCart(cartId.Value);
    }
}

internal class HandleConfirmCart:
    ICommandHandler<ConfirmShoppingCart>
{
    private readonly IEventStoreDBRepository<ShoppingCart> cartRepository;
    private readonly EventStoreDBOptimisticConcurrencyScope scope;

    public HandleConfirmCart(
        IEventStoreDBRepository<ShoppingCart> cartRepository,
        EventStoreDBOptimisticConcurrencyScope scope
    )
    {
        this.cartRepository = cartRepository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(ConfirmShoppingCart command, CancellationToken cancellationToken)
    {
        await scope.Do(expectedRevision =>
            cartRepository.GetAndUpdate(
                command.CartId,
                cart => cart.Confirm(),
                expectedRevision,
                cancellationToken
            )
        );

        return Unit.Value;
    }
}
