using System;
using System.Threading;
using System.Threading.Tasks;
using Carts.Carts.Products;
using Carts.Pricing;
using Core.Commands;
using Core.Repositories;
using MediatR;

namespace Carts.Carts.AddingProduct;

public class AddProduct: ICommand
{

    public Guid CartId { get; }

    public ProductItem ProductItem { get; }

    private AddProduct(Guid cartId, ProductItem productItem)
    {
        CartId = cartId;
        ProductItem = productItem;
    }
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
    private readonly IRepository<Cart> cartRepository;
    private readonly IProductPriceCalculator productPriceCalculator;

    public HandleAddProduct(
        IRepository<Cart> cartRepository,
        IProductPriceCalculator productPriceCalculator
    )
    {
        this.cartRepository = cartRepository;
        this.productPriceCalculator = productPriceCalculator;
    }

    public Task<Unit> Handle(AddProduct command, CancellationToken cancellationToken)
    {
        return cartRepository.GetAndUpdate(
            command.CartId,
            cart => cart.AddProduct(productPriceCalculator, command.ProductItem),
            cancellationToken);
    }
}