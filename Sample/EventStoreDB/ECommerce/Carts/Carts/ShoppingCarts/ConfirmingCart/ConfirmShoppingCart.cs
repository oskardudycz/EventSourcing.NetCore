using Core.Commands;
using Core.EventStoreDB.Repository;

namespace Carts.ShoppingCarts.ConfirmingCart;

public record ConfirmShoppingCart(
    Guid CartId
)
{
    public static ConfirmShoppingCart Create(Guid? cartId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new ConfirmShoppingCart(cartId.Value);
    }
}

internal class HandleConfirmCart(IEventStoreDBRepository<ShoppingCart> cartRepository):
    ICommandHandler<ConfirmShoppingCart>
{
    public Task Handle(ConfirmShoppingCart command, CancellationToken ct) =>
        cartRepository.GetAndUpdate(
            command.CartId,
            cart => cart.Confirm(),
            ct: ct
        );
}
