using Carts.ShoppingCarts;
using Core.Aggregates;

namespace Carts.Tests.Builders;

internal class CartBuilder
{
    private Func<ShoppingCart> build  = () => new ShoppingCart();

    public CartBuilder Opened()
    {
        var cartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        // When
        var cart = ShoppingCart.Open(
            cartId,
            clientId
        );

        build = () => cart;

        return this;
    }

    public static CartBuilder Create() => new();

    public ShoppingCart Build()
    {
        var cart = build();
        ((IAggregate)cart).DequeueUncommittedEvents();
        return cart;
    }
}
