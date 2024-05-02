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

internal class HandleConfirmShoppingCart(IMartenRepository<ShoppingCart> cartRepository):
    ICommandHandler<ConfirmShoppingCart>
{
    public Task Handle(ConfirmShoppingCart command, CancellationToken ct) =>
        cartRepository.GetAndUpdate(
            command.CartId,
            cart => cart.Confirm(),
            ct: ct
        );
}
