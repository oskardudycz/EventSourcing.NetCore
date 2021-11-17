using System;
using System.Threading;
using System.Threading.Tasks;
using Carts.Carts.Products;
using Core.Commands;
using Core.Repositories;
using MediatR;

namespace Carts.Carts.RemovingProduct;

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
    private readonly IRepository<Cart> cartRepository;

    public HandleRemoveProduct(
        IRepository<Cart> cartRepository
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