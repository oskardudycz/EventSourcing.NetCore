using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
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

    public HandleInitializeCart(
        IEventStoreDBRepository<ShoppingCart> cartRepository
    )
    {
        this.cartRepository = cartRepository;
    }

    public async Task<Unit> Handle(InitializeShoppingCart command, CancellationToken cancellationToken)
    {
        var cart = ShoppingCart.Initialize(command.CartId, command.ClientId);

        await cartRepository.Add(cart, cancellationToken);

        return Unit.Value;
    }
}
