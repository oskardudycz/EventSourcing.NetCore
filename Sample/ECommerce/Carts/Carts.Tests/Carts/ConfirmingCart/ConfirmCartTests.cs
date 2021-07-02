using FluentAssertions;
using Carts.Carts;
using Carts.Carts.ConfirmingCart;
using Carts.Tests.Builders;
using Core.Testing;
using Xunit;

namespace Carts.Tests.Carts
{
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
            cart.Status.Should().Be(CartStatus.Confirmed);
            cart.Version.Should().Be(2);

            var @event = cart.PublishedEvent<CartConfirmed>();

            @event.Should().NotBeNull();
            @event.Should().BeOfType<CartConfirmed>();
            @event!.CartId.Should().Be(cart.Id);
        }
    }
}
