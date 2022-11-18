using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;

namespace Carts.ShoppingCarts.ConfirmingCart;

public record ConfirmShoppingCart(
    Guid CartId
)
{
    public static ConfirmShoppingCart Create(Guid cartId)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new ConfirmShoppingCart(cartId);
    }
}

internal class HandleConfirmShoppingCart:
    ICommandHandler<ConfirmShoppingCart>
{
    private readonly IMartenRepository<ShoppingCart> cartRepository;
    private readonly IMartenAppendScope scope;

    public HandleConfirmShoppingCart(
        IMartenRepository<ShoppingCart> cartRepository,
        IMartenAppendScope scope
    )
    {
        this.cartRepository = cartRepository;
        this.scope = scope;
    }

    public async Task Handle(ConfirmShoppingCart command, CancellationToken cancellationToken)
    {
        await scope.Do((expectedVersion, traceMetadata) =>
            cartRepository.GetAndUpdate(
                command.CartId,
                cart => cart.Confirm(),
                expectedVersion,
                traceMetadata,
                cancellationToken
            )
        );
    }
}
