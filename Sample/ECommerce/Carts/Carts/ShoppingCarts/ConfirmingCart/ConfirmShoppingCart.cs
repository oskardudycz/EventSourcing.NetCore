using Core.Commands;
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

    public HandleConfirmShoppingCart(IMartenRepository<ShoppingCart> cartRepository) =>
        this.cartRepository = cartRepository;

    public Task Handle(ConfirmShoppingCart command, CancellationToken cancellationToken) =>
        cartRepository.GetAndUpdate(
            command.CartId,
            cart => cart.Confirm(),
            cancellationToken: cancellationToken
        );
}
