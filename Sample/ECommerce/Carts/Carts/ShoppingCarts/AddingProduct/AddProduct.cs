using Carts.Pricing;
using Carts.ShoppingCarts.Products;
using Core.Commands;
using Core.Marten.Repository;

namespace Carts.ShoppingCarts.AddingProduct;

public record AddProduct(
    Guid CartId,
    ProductItem ProductItem
)
{
    public static AddProduct Create(Guid cartId, ProductItem productItem)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new AddProduct(cartId, productItem);
    }
}

internal class HandleAddProduct:
    ICommandHandler<AddProduct>
{
    private readonly IMartenRepository<ShoppingCart> cartRepository;
    private readonly IProductPriceCalculator productPriceCalculator;

    public HandleAddProduct(
        IMartenRepository<ShoppingCart> cartRepository,
        IProductPriceCalculator productPriceCalculator
    )
    {
        this.cartRepository = cartRepository;
        this.productPriceCalculator = productPriceCalculator;
    }

    public Task Handle(AddProduct command, CancellationToken ct)
    {
        var (cartId, productItem) = command;

        return cartRepository.GetAndUpdate(
            cartId,
            cart => cart.AddProduct(productPriceCalculator, productItem),
            ct: ct
        );
    }
}
