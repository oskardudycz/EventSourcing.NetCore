using System;
using Carts.Carts.AddingProduct;
using Carts.Carts.ConfirmingCart;
using Carts.Carts.InitializingCart;
using Carts.Carts.RemovingProduct;
using Core.Projections;

namespace Carts.Carts.GettingCartHistory;

public class CartHistory: IProjection
{
    public Guid Id { get; set;}
    public Guid CartId { get; set;}
    public string Description { get; set; } = default!;

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

    public void Apply(CartConfirmed @event)
    {
        Id = Guid.NewGuid();
        CartId = @event.CartId;
        Description = $"Created tentative Cart with id {@event.CartId}";
    }
}