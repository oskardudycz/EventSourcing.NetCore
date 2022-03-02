using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Marten.Repository;
using MediatR;

namespace Carts.ShoppingCarts.ConfirmingCart;

public class ConfirmShoppingCart: ICommand
{
    public Guid CartId { get; }

    private ConfirmShoppingCart(Guid cartId)
    {
        CartId = cartId;
    }

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

    public HandleConfirmCart(
        IMartenRepository<ShoppingCart> cartRepository
    )
    {
        this.cartRepository = cartRepository;
    }

    public Task<Unit> Handle(ConfirmShoppingCart command, CancellationToken cancellationToken)
    {
        return cartRepository.GetAndUpdate(
            command.CartId,
            cart => cart.Confirm(),
            cancellationToken);
    }
}
