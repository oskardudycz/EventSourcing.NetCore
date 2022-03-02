using System;
using Carts.ShoppingCarts.AddingProduct;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.InitializingCart;
using Carts.ShoppingCarts.RemovingProduct;
using Marten.Events.Aggregation;

namespace Carts.ShoppingCarts.GettingCarts;

public class ShoppingCartShortInfo
{
    public Guid Id { get; set; }

    public int TotalItemsCount { get; set; }

    public ShoppingCartStatus Status { get; set; }

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

public class CartShortInfoProjection : AggregateProjection<ShoppingCartShortInfo>
{
    public CartShortInfoProjection()
    {
        ProjectEvent<ShoppingCartInitialized>((item, @event) => item.Apply(@event));

        ProjectEvent<ProductAdded>((item, @event) => item.Apply(@event));

        ProjectEvent<ProductRemoved>((item, @event) => item.Apply(@event));

        ProjectEvent<ShoppingCartConfirmed>((item, @event) => item.Apply(@event));
    }
}
