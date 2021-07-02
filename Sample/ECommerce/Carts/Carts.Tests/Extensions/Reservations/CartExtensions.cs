using System;
using Carts.Carts;
using Carts.Carts.InitializingCart;
using Core.Testing;
using FluentAssertions;

namespace Carts.Tests.Extensions.Reservations
{
    internal static class CartExtensions
    {
        public static Cart IsInitializedCartWith(
            this Cart cart,
            Guid id,
            Guid clientId)
        {

            cart.Id.Should().Be(id);
            cart.ClientId.Should().Be(clientId);
            cart.Status.Should().Be(CartStatus.Pending);
            cart.ProductItems.Should().BeEmpty();
            cart.TotalPrice.Should().Be(0);
            cart.Version.Should().Be(1);

            return cart;
        }

        public static Cart HasCartInitializedEventWith(
            this Cart cart,
            Guid id,
            Guid clientId)
        {
            var @event = cart.PublishedEvent<CartInitialized>();

            @event.Should().NotBeNull();
            @event.Should().BeOfType<CartInitialized>();
            @event!.CartId.Should().Be(id);
            @event.ClientId.Should().Be(clientId);
            @event.CartStatus.Should().Be(CartStatus.Pending);

            return cart;
        }
    }
}
