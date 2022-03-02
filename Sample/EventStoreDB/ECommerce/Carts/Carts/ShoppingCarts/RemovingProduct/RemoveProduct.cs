using System;
using System.Threading;
using System.Threading.Tasks;
using Carts.ShoppingCarts.Products;
using Core.Commands;
using Core.EventStoreDB.Repository;
using MediatR;

namespace Carts.ShoppingCarts.RemovingProduct;

public class RemoveProduct: ICommand
{
    public Guid CartId { get; }

    public PricedProductItem ProductItem { get; }

    private RemoveProduct(Guid cartId, PricedProductItem productItem)
    {
        CartId = cartId;
        ProductItem = productItem;
    }

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

    public HandleRemoveProduct(
        IEventStoreDBRepository<ShoppingCart> cartRepository
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
