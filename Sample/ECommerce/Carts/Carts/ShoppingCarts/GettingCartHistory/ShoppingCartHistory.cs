using Carts.ShoppingCarts.AddingProduct;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.ShoppingCarts.InitializingCart;
using Carts.ShoppingCarts.RemovingProduct;
using Marten.Events;
using Marten.Events.Projections;

namespace Carts.ShoppingCarts.GettingCartHistory;

public record ShoppingCartHistory (
    Guid Id,
    Guid CartId,
    string Description
);

public class CartHistoryTransformation : EventProjection
{
    public ShoppingCartHistory Transform(IEvent<ShoppingCartInitialized> input)
    {
        return new (
            Guid.NewGuid(),
            input.Data.CartId,
            $"Created tentative Cart with id {input.Data.CartId}"
        );
    }

    public ShoppingCartHistory Transform(IEvent<ProductAdded> input)
    {
        return new (
            Guid.NewGuid(),
            input.Data.CartId,
            $"Added {input.Data.ProductItem.Quantity} Product with id `{input.Data.ProductItem.ProductId}` to Cart `{input.Data.CartId}`"
        );
    }

    public ShoppingCartHistory Transform(IEvent<ProductRemoved> input)
    {
        return new (
            Guid.NewGuid(),
            input.Data.CartId,
            $"Removed Product {input.Data.ProductItem.Quantity} with id `{input.Data.ProductItem.ProductId}` to Cart `{input.Data.CartId}`"
        );
    }

    public ShoppingCartHistory Transform(IEvent<ShoppingCartConfirmed> input)
    {
        return new (
            Guid.NewGuid(),
            input.Data.CartId,
            $"Confirmed Cart with id `{input.Data.CartId}`"
        );
    }
}
