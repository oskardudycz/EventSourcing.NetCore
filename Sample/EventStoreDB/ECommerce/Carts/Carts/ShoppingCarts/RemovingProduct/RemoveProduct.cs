using System;
using System.Threading;
using System.Threading.Tasks;
using Carts.ShoppingCarts.AddingProduct;
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

        return new(cartId.Value, productItem);
    }
}

internal class HandleRemoveProduct:
    ICommandHandler<RemoveProduct>
{
    private readonly IEventStoreDBRepository<ShoppingCart> cartRepository;
    private readonly EventStoreDBOptimisticConcurrencyScope scope;

    public HandleRemoveProduct(
        IEventStoreDBRepository<ShoppingCart> cartRepository,
        EventStoreDBOptimisticConcurrencyScope scope
    )
    {
        this.cartRepository = cartRepository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(RemoveProduct command, CancellationToken cancellationToken)
    {
        await scope.Do(expectedRevision =>
            cartRepository.GetAndUpdate(
                command.CartId,
                cart => cart.RemoveProduct(command.ProductItem),
                expectedRevision,
                cancellationToken
            )
        );

        return Unit.Value;
    }
}
