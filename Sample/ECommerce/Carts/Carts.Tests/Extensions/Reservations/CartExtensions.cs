using Carts.ShoppingCarts;
using Carts.ShoppingCarts.OpeningCart;
using Core.Testing;
using Shouldly;

namespace Carts.Tests.Extensions.Reservations;

internal static class CartExtensions
{
    public static ShoppingCart IsOpenedCartWith(
        this ShoppingCart shoppingCart,
        Guid id,
        Guid clientId)
    {
        shoppingCart.Id.ShouldBe(id);
        shoppingCart.ClientId.ShouldBe(clientId);
        shoppingCart.Status.ShouldBe(ShoppingCartStatus.Pending);
        shoppingCart.ProductItems.ShouldBeEmpty();
        shoppingCart.TotalPrice.ShouldBe(0);

        return shoppingCart;
    }

    public static ShoppingCart HasCartOpenedEventWith(
        this ShoppingCart shoppingCart,
        Guid id,
        Guid clientId)
    {
        var @event = shoppingCart.PublishedEvent<ShoppingCartOpened>();

        @event.ShouldNotBeNull();
        @event.ShouldBeOfType<ShoppingCartOpened>();
        @event!.CartId.ShouldBe(id);
        @event.ClientId.ShouldBe(clientId);

        return shoppingCart;
    }
}
