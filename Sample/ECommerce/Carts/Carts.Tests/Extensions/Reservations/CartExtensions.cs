using Carts.ShoppingCarts;
using Carts.ShoppingCarts.OpeningCart;
using Core.Testing;
using FluentAssertions;

namespace Carts.Tests.Extensions.Reservations;

internal static class CartExtensions
{
    public static ShoppingCart IsOpenedCartWith(
        this ShoppingCart shoppingCart,
        Guid id,
        Guid clientId)
    {

        shoppingCart.Id.Should().Be(id);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Pending);
        shoppingCart.ProductItems.Should().BeEmpty();
        shoppingCart.TotalPrice.Should().Be(0);

        return shoppingCart;
    }

    public static ShoppingCart HasCartOpenedEventWith(
        this ShoppingCart shoppingCart,
        Guid id,
        Guid clientId)
    {
        var @event = shoppingCart.PublishedEvent<ShoppingCartOpened>();

        @event.Should().NotBeNull();
        @event.Should().BeOfType<ShoppingCartOpened>();
        @event!.CartId.Should().Be(id);
        @event.ClientId.Should().Be(clientId);

        return shoppingCart;
    }
}
