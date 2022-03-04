using Core.Commands;
using Core.Marten.Events;
using Core.Marten.OptimisticConcurrency;
using Core.Marten.Repository;
using MediatR;

namespace Carts.ShoppingCarts.ConfirmingCart;

public record ConfirmShoppingCart(
    Guid CartId
): ICommand
{
    public static ConfirmShoppingCart Create(Guid cartId)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new ConfirmShoppingCart(cartId);
    }
}

internal class HandleConfirmCart:
    ICommandHandler<ConfirmShoppingCart>
{
    private readonly IMartenRepository<ShoppingCart> cartRepository;
    private readonly IMartenAppendScope scope;

    public HandleConfirmCart(
        IMartenRepository<ShoppingCart> cartRepository,
        IMartenAppendScope scope
    )
    {
        this.cartRepository = cartRepository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(ConfirmShoppingCart command, CancellationToken cancellationToken)
    {
        await scope.Do((expectedVersion, eventMetadata) =>
            cartRepository.GetAndUpdate(
                command.CartId,
                cart => cart.Confirm(),
                expectedVersion,
                eventMetadata,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
