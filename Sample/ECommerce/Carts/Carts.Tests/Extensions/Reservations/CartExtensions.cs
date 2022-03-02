using System;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.InitializingCart;
using Core.Testing;
using FluentAssertions;

namespace Carts.Tests.Extensions.Reservations;

internal static class CartExtensions
{
    public static ShoppingCart IsInitializedCartWith(
        this ShoppingCart shoppingCart,
        Guid id,
        Guid clientId)
    {

        shoppingCart.Id.Should().Be(id);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Pending);
        shoppingCart.ProductItems.Should().BeEmpty();
        shoppingCart.TotalPrice.Should().Be(0);
        shoppingCart.Version.Should().Be(1);

        return shoppingCart;
    }

    public static ShoppingCart HasCartInitializedEventWith(
        this ShoppingCart shoppingCart,
        Guid id,
        Guid clientId)
    {
        var @event = shoppingCart.PublishedEvent<ShoppingCartInitialized>();

        @event.Should().NotBeNull();
        @event.Should().BeOfType<ShoppingCartInitialized>();
        @event!.CartId.Should().Be(id);
        @event.ClientId.Should().Be(clientId);
        @event.ShoppingCartStatus.Should().Be(ShoppingCartStatus.Pending);

        return shoppingCart;
    }
}
