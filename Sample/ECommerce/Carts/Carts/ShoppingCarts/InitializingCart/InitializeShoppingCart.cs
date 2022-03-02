using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Marten.Repository;
using MediatR;

namespace Carts.ShoppingCarts.InitializingCart;

public class InitializeShoppingCart: ICommand
{
    public Guid CartId { get; }

    public Guid ClientId { get; }

    private InitializeShoppingCart(Guid cartId, Guid clientId)
    {
        CartId = cartId;
        ClientId = clientId;
    }

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
    private readonly IMartenRepository<ShoppingCart> cartRepository;

    public HandleInitializeCart(
        IMartenRepository<ShoppingCart> cartRepository
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
