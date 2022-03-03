using Core.Commands;
using Core.EventStoreDB.OptimisticConcurrency;
using Core.EventStoreDB.Repository;
using MediatR;

namespace Carts.ShoppingCarts.InitializingCart;

public record InitializeShoppingCart(
    Guid CartId,
    Guid ClientId
): ICommand
{
    public static InitializeShoppingCart Create(Guid? cartId, Guid? clientId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (clientId == null || clientId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(clientId));

        return new InitializeShoppingCart(cartId.Value, clientId.Value);
    }
}

internal class HandleInitializeCart:
    ICommandHandler<InitializeShoppingCart>
{
    private readonly IEventStoreDBRepository<ShoppingCart> cartRepository;
    private readonly EventStoreDBOptimisticConcurrencyScope scope;

    public HandleInitializeCart(
        IEventStoreDBRepository<ShoppingCart> cartRepository,
        EventStoreDBOptimisticConcurrencyScope scope
    )
    {
        this.cartRepository = cartRepository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(InitializeShoppingCart command, CancellationToken cancellationToken)
    {
        var (cartId, clientId) = command;

        await scope.Do(_ =>
            cartRepository.Add(
                ShoppingCart.Initialize(cartId, clientId),
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
