using Carts.ShoppingCarts;
using Carts.ShoppingCarts.OpeningCart;

namespace Carts.Tests.Builders;

internal class CartBuilder
{
    private readonly Queue<object> eventsToApply = new();

    public CartBuilder Opened()
    {
        var cartId = Guid.CreateVersion7();
        var clientId = Guid.CreateVersion7();

        eventsToApply.Enqueue(new ShoppingCartOpened(cartId, clientId, ShoppingCartStatus.Pending));

        return this;
    }

    public static CartBuilder Create() => new();

    public ShoppingCart Build()
    {
        var cart = (ShoppingCart) Activator.CreateInstance(typeof(ShoppingCart), true)!;

        foreach (var @event in eventsToApply)
        {
            cart.Apply(@event);
        }

        return cart;
    }
}
