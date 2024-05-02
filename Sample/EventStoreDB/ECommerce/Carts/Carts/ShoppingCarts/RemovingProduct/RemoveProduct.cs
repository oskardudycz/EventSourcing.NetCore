using Carts.ShoppingCarts.Products;
using Core.Commands;
using Core.EventStoreDB.Repository;

namespace Carts.ShoppingCarts.RemovingProduct;

public record RemoveProduct(
    Guid CartId,
    PricedProductItem ProductItem
)
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

internal class HandleRemoveProduct(IEventStoreDBRepository<ShoppingCart> cartRepository):
    ICommandHandler<RemoveProduct>
{
    public Task Handle(RemoveProduct command, CancellationToken ct)
    {
        var (cartId, pricedProductItem) = command;

        return cartRepository.GetAndUpdate(
            cartId,
            cart => cart.RemoveProduct(pricedProductItem),
            ct: ct
        );
    }
}
