using System;
using Carts.Carts.AddingProduct;
using Carts.Carts.ConfirmingCart;
using Carts.Carts.InitializingCart;
using Carts.Carts.RemovingProduct;
using Marten.Events.Aggregation;

namespace Carts.Carts.GettingCarts;

public class CartShortInfo
{
    public Guid Id { get; set; }

    public int TotalItemsCount { get; set; }

    public CartStatus Status { get; set; }

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

public class CartShortInfoProjection : AggregateProjection<CartShortInfo>
{
    public CartShortInfoProjection()
    {
        ProjectEvent<CartInitialized>((item, @event) => item.Apply(@event));

        ProjectEvent<ProductAdded>((item, @event) => item.Apply(@event));

        ProjectEvent<ProductRemoved>((item, @event) => item.Apply(@event));

        ProjectEvent<CartConfirmed>((item, @event) => item.Apply(@event));
    }
}