using Carts.Pricing;
using Carts.ShoppingCarts.Products;
using Core.Commands;
using Core.EventStoreDB.OptimisticConcurrency;
using Core.EventStoreDB.Repository;
using MediatR;

namespace Carts.ShoppingCarts.AddingProduct;

public record AddProduct(
    Guid CartId,
    ProductItem ProductItem
): ICommand
{
    public static AddProduct Create(Guid? cartId, ProductItem? productItem)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (productItem == null)
            throw new ArgumentOutOfRangeException(nameof(productItem));

        return new AddProduct(cartId.Value, productItem);
    }
}

internal class HandleAddProduct:
    ICommandHandler<AddProduct>
{
    private readonly IEventStoreDBRepository<ShoppingCart> cartRepository;
    private readonly IProductPriceCalculator productPriceCalculator;
    private readonly IEventStoreDBAppendScope scope;

    public HandleAddProduct(
        IEventStoreDBRepository<ShoppingCart> cartRepository,
        IProductPriceCalculator productPriceCalculator,
        IEventStoreDBAppendScope scope
    )
    {
        this.cartRepository = cartRepository;
        this.productPriceCalculator = productPriceCalculator;
        this.scope = scope;
    }

    public async Task<Unit> Handle(AddProduct command, CancellationToken cancellationToken)
    {
        var (cartId, productItem) = command;

        await scope.Do((expectedRevision, eventMetadata) =>
            cartRepository.GetAndUpdate(
                cartId,
                cart => cart.AddProduct(productPriceCalculator, productItem),
                expectedRevision,
                eventMetadata,
                cancellationToken
            )
        );

        return Unit.Value;
    }
}
