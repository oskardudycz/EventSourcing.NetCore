using Carts.ShoppingCarts;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.Tests.Builders;
using Core.Testing;
using FluentAssertions;
using Xunit;

namespace Carts.Tests.Carts.ConfirmingCart;

public class ConfirmCartTests
{
    [Fact]
    public void ForTentativeCart_ShouldSucceed()
    {
        // Given
        var cart = CartBuilder
            .Create()
            .Initialized()
            .Build();

        // When
        cart.Confirm();

        // Then
        cart.Status.Should().Be(ShoppingCartStatus.Confirmed);
        cart.Version.Should().Be(1);

        var @event = cart.PublishedEvent<ShoppingCartConfirmed>();

        @event.Should().NotBeNull();
        @event.Should().BeOfType<ShoppingCartConfirmed>();
        @event!.CartId.Should().Be(cart.Id);
    }
}
