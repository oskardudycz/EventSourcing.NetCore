using System;
using Carts.ShoppingCarts.AddingProduct;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.InitializingCart;
using Carts.ShoppingCarts.RemovingProduct;
using Core.Projections;

namespace Carts.ShoppingCarts.GettingCartHistory;

public class CartHistory: IProjection
{
    public Guid Id { get; set;}
    public Guid CartId { get; set;}
    public string Description { get; set; } = default!;

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
        Id = Guid.NewGuid();
        CartId = @event.CartId;
        Description = $"Created tentative Cart with id {@event.CartId}";
    }

    public void Apply(ProductAdded @event)
    {
        Id = Guid.NewGuid();
        CartId = @event.CartId;
        Description = $"Created tentative Cart with id {@event.CartId}";
    }

    public void Apply(ProductRemoved @event)
    {
        Id = Guid.NewGuid();
        CartId = @event.CartId;
        Description = $"Created tentative Cart with id {@event.CartId}";
    }

    public void Apply(ShoppingCartConfirmed @event)
    {
        Id = Guid.NewGuid();
        CartId = @event.CartId;
        Description = $"Created tentative Cart with id {@event.CartId}";
    }
}