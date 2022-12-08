using Core.Commands;
using Core.Marten.Repository;

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

internal class HandleOpenShoppingCart:
    ICommandHandler<OpenShoppingCart>
{
    private readonly IMartenRepository<ShoppingCart> cartRepository;

    public HandleOpenShoppingCart(IMartenRepository<ShoppingCart> cartRepository) =>
        this.cartRepository = cartRepository;

    public Task Handle(OpenShoppingCart command, CancellationToken cancellationToken)
    {
        var (cartId, clientId) = command;

        return cartRepository.Add(
            ShoppingCart.Open(cartId, clientId),
            cancellationToken
        );
    }
}
