using Core.Commands;
using Core.EventStoreDB.OptimisticConcurrency;
using Core.EventStoreDB.Repository;
using MediatR;

namespace Carts.ShoppingCarts.OpeningCart;

public record OpenShoppingCart(
    Guid CartId,
    Guid ClientId
)
{
    public static OpenShoppingCart Create(Guid? cartId, Guid? clientId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (clientId == null || clientId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(clientId));

        return new OpenShoppingCart(cartId.Value, clientId.Value);
    }
}

internal class HandleOpenCart:
    ICommandHandler<OpenShoppingCart>
{
    private readonly IEventStoreDBRepository<ShoppingCart> cartRepository;
    private readonly IEventStoreDBAppendScope scope;

    public HandleOpenCart(
        IEventStoreDBRepository<ShoppingCart> cartRepository,
        IEventStoreDBAppendScope scope
    )
    {
        this.cartRepository = cartRepository;
        this.scope = scope;
    }

    public async Task Handle(OpenShoppingCart command, CancellationToken cancellationToken)
    {
        var (cartId, clientId) = command;

        await scope.Do((_, eventMetadata) =>
            cartRepository.Add(
                ShoppingCart.Open(cartId, clientId),
                eventMetadata,
                cancellationToken
            )
        );
    }
}
