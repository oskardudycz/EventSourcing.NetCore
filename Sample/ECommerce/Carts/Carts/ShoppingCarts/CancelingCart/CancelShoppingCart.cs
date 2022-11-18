using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;

namespace Carts.ShoppingCarts.CancelingCart;

public record CancelShoppingCart(
    Guid CartId
)
{
    public static CancelShoppingCart Create(Guid cartId)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new CancelShoppingCart(cartId);
    }
}

internal class HandleCancelShoppingCart:
    ICommandHandler<CancelShoppingCart>
{
    private readonly IMartenRepository<ShoppingCart> cartRepository;
    private readonly IMartenAppendScope scope;

    public HandleCancelShoppingCart(
        IMartenRepository<ShoppingCart> cartRepository,
        IMartenAppendScope scope
    )
    {
        this.cartRepository = cartRepository;
        this.scope = scope;
    }

    public async Task Handle(CancelShoppingCart command, CancellationToken cancellationToken)
    {
        await scope.Do((expectedVersion, traceMetadata) =>
            cartRepository.GetAndUpdate(
                command.CartId,
                cart => cart.Cancel(),
                expectedVersion,
                traceMetadata,
                cancellationToken
            )
        );
    }
}
