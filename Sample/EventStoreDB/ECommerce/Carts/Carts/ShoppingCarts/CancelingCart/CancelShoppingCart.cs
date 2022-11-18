using Core.Commands;
using Core.EventStoreDB.OptimisticConcurrency;
using Core.EventStoreDB.Repository;

namespace Carts.ShoppingCarts.CancelingCart;

public record CancelShoppingCart(
    Guid CartId
)
{
    public static CancelShoppingCart Create(Guid? cartId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new CancelShoppingCart(cartId.Value);
    }
}

internal class HandleCancelCart:
    ICommandHandler<CancelShoppingCart>
{
    private readonly IEventStoreDBRepository<ShoppingCart> cartRepository;
    private readonly IEventStoreDBAppendScope scope;

    public HandleCancelCart(
        IEventStoreDBRepository<ShoppingCart> cartRepository,
        IEventStoreDBAppendScope scope
    )
    {
        this.cartRepository = cartRepository;
        this.scope = scope;
    }

    public async Task Handle(CancelShoppingCart command, CancellationToken cancellationToken)
    {
        await scope.Do((expectedRevision, eventMetadata) =>
            cartRepository.GetAndUpdate(
                command.CartId,
                cart => cart.Cancel(),
                expectedRevision,
                eventMetadata,
                cancellationToken
            )
        );
    }
}
