using Carts.ShoppingCarts.Products;
using Core.Commands;
using Core.EventStoreDB.OptimisticConcurrency;
using Core.EventStoreDB.Repository;
using MediatR;

namespace Carts.ShoppingCarts.RemovingProduct;

public record RemoveProduct(
    Guid CartId,
    PricedProductItem ProductItem
): ICommand
{
    public static RemoveProduct Create(Guid? cartId, PricedProductItem? productItem)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (productItem == null)
            throw new ArgumentOutOfRangeException(nameof(productItem));

        return new RemoveProduct(cartId.Value, productItem);
    }
}

internal class HandleRemoveProduct:
    ICommandHandler<RemoveProduct>
{
    private readonly IEventStoreDBRepository<ShoppingCart> cartRepository;
    private readonly IEventStoreDBAppendScope scope;

    public HandleRemoveProduct(
        IEventStoreDBRepository<ShoppingCart> cartRepository,
        IEventStoreDBAppendScope scope
    )
    {
        this.cartRepository = cartRepository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(RemoveProduct command, CancellationToken cancellationToken)
    {
        var (cartId, pricedProductItem) = command;

        await scope.Do((expectedRevision, eventMetadata) =>
            cartRepository.GetAndUpdate(
                cartId,
                cart => cart.RemoveProduct(pricedProductItem),
                expectedRevision,
                eventMetadata,
                cancellationToken
            )
        );

        return Unit.Value;
    }
}
