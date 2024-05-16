using Core.Commands;
using Core.EventStoreDB.Repository;

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

internal class HandleOpenCart(IEventStoreDBRepository<ShoppingCart> cartRepository):
    ICommandHandler<OpenShoppingCart>
{
    public Task Handle(OpenShoppingCart command, CancellationToken ct)
    {
        var (cartId, clientId) = command;

        return cartRepository.Add(
            cartId,
            ShoppingCart.Open(cartId, clientId),
            ct
        );
    }
}
