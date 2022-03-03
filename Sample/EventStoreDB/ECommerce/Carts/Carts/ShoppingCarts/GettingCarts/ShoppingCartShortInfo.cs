using Carts.ShoppingCarts.AddingProduct;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.InitializingCart;
using Carts.ShoppingCarts.RemovingProduct;
using Core.Projections;

namespace Carts.ShoppingCarts.GettingCarts;

public class ShoppingCartShortInfo: IVersionedProjection
{
    public Guid Id { get; set; }

    public int TotalItemsCount { get; set; }

    public ShoppingCartStatus Status { get; set; }

    public ulong LastProcessedPosition { get; set; }

    public void When(object @event)
    {
        switch (@event)
        {
            case ShoppingCartInitialized cartInitialized:
                Apply(cartInitialized);
                return;
            case ProductAdded cartInitialized:
                Apply(cartInitialized);
                return;
            case ProductRemoved cartInitialized:
                Apply(cartInitialized);
                return;
            case ShoppingCartConfirmed cartInitialized:
                Apply(cartInitialized);
                return;
        }
    }

    public void Apply(ShoppingCartInitialized @event)
    {
        Id = @event.CartId;
        TotalItemsCount = 0;
        Status = ShoppingCartStatus.Pending;
    }

    public void Apply(ProductAdded @event)
    {
        TotalItemsCount += @event.ProductItem.Quantity;
    }

    public void Apply(ProductRemoved @event)
    {
        TotalItemsCount -= @event.ProductItem.Quantity;
    }

    public void Apply(ShoppingCartConfirmed @event)
    {
        Status = ShoppingCartStatus.Confirmed;
    }
}
