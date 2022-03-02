using System;
using System.Threading;
using System.Threading.Tasks;
using Carts.ShoppingCarts.Products;
using Core.Commands;
using Core.Marten.Repository;
using MediatR;

namespace Carts.ShoppingCarts.RemovingProduct;

public class RemoveProduct: ICommand
{
    public Guid CartId { get; }

    public PricedProductItem ProductItem { get; }

    private RemoveProduct(Guid cardId, PricedProductItem productItem)
    {
        CartId = cardId;
        ProductItem = productItem;
    }

    public static RemoveProduct Create(Guid cardId, PricedProductItem productItem)
    {
        return new(cardId, productItem);
    }
}

internal class HandleRemoveProduct:
    ICommandHandler<RemoveProduct>
{
    private readonly IMartenRepository<ShoppingCart> cartRepository;

    public HandleRemoveProduct(
        IMartenRepository<ShoppingCart> cartRepository
    )
    {
        this.cartRepository = cartRepository;
    }

    public Task<Unit> Handle(RemoveProduct command, CancellationToken cancellationToken)
    {
        return cartRepository.GetAndUpdate(
            command.CartId,
            cart => cart.RemoveProduct(command.ProductItem),
            cancellationToken);
    }
}
