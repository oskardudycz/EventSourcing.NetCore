using Core.Commands;
using Core.KurrentDB.Repository;

namespace Carts.ShoppingCarts.CancelingCart;

public record CancelShoppingCart(
    Guid CartId
)
{
    public static CancelShoppingCart Create(Guid? cartId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new CancelShoppingCart(cartId.Value);
    }
}

internal class HandleCancelCart(IKurrentDBRepository<ShoppingCart> cartRepository):
    ICommandHandler<CancelShoppingCart>
{
    public Task Handle(CancelShoppingCart command, CancellationToken ct) =>
        cartRepository.GetAndUpdate(
            command.CartId,
            cart => cart.Cancel(),
            ct: ct
        );
}
