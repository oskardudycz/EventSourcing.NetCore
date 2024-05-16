using Carts.ShoppingCarts.AddingProduct;
using Carts.ShoppingCarts.CancelingCart;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.OpeningCart;
using Carts.ShoppingCarts.RemovingProduct;
using Core.Projections;

namespace Carts.ShoppingCarts.GettingCartHistory;

public class CartHistory: IVersionedProjection
{
    public Guid Id { get; set;}
    public Guid CartId { get; set;}
    public string Description { get; set; } = default!;
    public ulong LastProcessedPosition { get; set; }

    public void Apply(object @event)
    {
        switch (@event)
        {
            case ShoppingCartOpened cartOpened:
                Apply(cartOpened);
                return;
            case ProductAdded cartOpened:
                Apply(cartOpened);
                return;
            case ProductRemoved cartOpened:
                Apply(cartOpened);
                return;
            case ShoppingCartConfirmed cartOpened:
                Apply(cartOpened);
                return;
            case ShoppingCartCanceled cartCanceled:
                Apply(cartCanceled);
                return;
        }
    }

    public void Apply(ShoppingCartOpened @event)
    {
        Id = Guid.NewGuid();
        CartId = @event.CartId;
        Description = $"Opened Cart with id {@event.CartId}";
    }

    public void Apply(ProductAdded @event)
    {
        Id = Guid.NewGuid();
        CartId = @event.CartId;
        Description = $"Added Product with id {@event.ProductItem.ProductId} to Cart with id {@event.CartId}";
    }

    public void Apply(ProductRemoved @event)
    {
        Id = Guid.NewGuid();
        CartId = @event.CartId;
        Description = $"Removed Product with id {@event.ProductItem.ProductId} from Cart with id {@event.CartId}";
    }

    public void Apply(ShoppingCartConfirmed @event)
    {
        Id = Guid.NewGuid();
        CartId = @event.CartId;
        Description = $"Confirmed Cart with id {@event.CartId}";
    }

    public void Apply(ShoppingCartCanceled @event)
    {
        Id = Guid.NewGuid();
        CartId = @event.CartId;
        Description = $"Canceled Cart with id {@event.CartId}";
    }
}
