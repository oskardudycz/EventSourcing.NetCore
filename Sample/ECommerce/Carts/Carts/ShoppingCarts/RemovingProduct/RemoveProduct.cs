using Carts.ShoppingCarts.Products;
using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;

namespace Carts.ShoppingCarts.RemovingProduct;

public record RemoveProduct(
    Guid CartId,
    PricedProductItem ProductItem
)
{
    public static RemoveProduct Create(Guid cartId, PricedProductItem productItem)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new RemoveProduct(cartId, productItem);
    }
}

internal class HandleRemoveProduct:
    ICommandHandler<RemoveProduct>
{
    private readonly IMartenRepository<ShoppingCart> cartRepository;
    private readonly IMartenAppendScope scope;

    public HandleRemoveProduct(
        IMartenRepository<ShoppingCart> cartRepository,
        IMartenAppendScope scope
    )
    {
        this.cartRepository = cartRepository;
        this.scope = scope;
    }

    public async Task Handle(RemoveProduct command, CancellationToken cancellationToken)
    {
        var (cartId, productItem) = command;

        await scope.Do(expectedVersion =>
            cartRepository.GetAndUpdate(
                cartId,
                cart => cart.RemoveProduct(productItem),
                expectedVersion,
                cancellationToken
            )
        );
    }
}
