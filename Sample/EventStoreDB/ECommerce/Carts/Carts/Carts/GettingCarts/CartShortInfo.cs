using System;
using Carts.Carts.AddingProduct;
using Carts.Carts.ConfirmingCart;
using Carts.Carts.InitializingCart;
using Carts.Carts.RemovingProduct;
using Core.Projections;

namespace Carts.Carts.GettingCarts;

public class CartShortInfo: IProjection
{
    public Guid Id { get; set; }

    public int TotalItemsCount { get; set; }

    public CartStatus Status { get; set; }

    public void When(object @event)
    {
        switch (@event)
        {
            case CartInitialized cartInitialized:
                Apply(cartInitialized);
                return;
            case ProductAdded cartInitialized:
                Apply(cartInitialized);
                return;
            case ProductRemoved cartInitialized:
                Apply(cartInitialized);
                return;
            case CartConfirmed cartInitialized:
                Apply(cartInitialized);
                return;
        }
    }

    public void Apply(CartInitialized @event)
    {
        Id = @event.CartId;
        TotalItemsCount = 0;
        Status = CartStatus.Pending;
    }

    public void Apply(ProductAdded @event)
    {
        TotalItemsCount += @event.ProductItem.Quantity;
    }

    public void Apply(ProductRemoved @event)
    {
        TotalItemsCount -= @event.ProductItem.Quantity;
    }

    public void Apply(CartConfirmed @event)
    {
        Status = CartStatus.Confirmed;
    }
}