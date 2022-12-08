using Core.Commands;
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

    public HandleCancelShoppingCart(IMartenRepository<ShoppingCart> cartRepository) =>
        this.cartRepository = cartRepository;

    public Task Handle(CancelShoppingCart command, CancellationToken cancellationToken) =>
        cartRepository.GetAndUpdate(
            command.CartId,
            cart => cart.Cancel(),
            cancellationToken: cancellationToken
        );
}
