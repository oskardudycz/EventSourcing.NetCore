using Carts.ShoppingCarts;
using Carts.ShoppingCarts.ConfirmingCart;
using Carts.Tests.Builders;
using Core.Testing;
using Shouldly;
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
            .Opened()
            .WithProduct()
            .Build();

        // When
        cart.Confirm();

        // Then
        cart.Status.ShouldBe(ShoppingCartStatus.Confirmed);

        var @event = cart.PublishedEvent<ShoppingCartConfirmed>();

        @event.ShouldNotBeNull();
        @event.ShouldBeOfType<ShoppingCartConfirmed>();
        @event!.CartId.ShouldBe(cart.Id);
    }

    [Fact]
    public void ForEmptyCart_ShouldFail()
    {
        // Given
        var emptyCart = CartBuilder
            .Create()
            .Opened()
            .Build();

        // When
        Action confirmAction = () => emptyCart.Confirm();

        // Then
        confirmAction.ShouldThrow<InvalidOperationException>();
    }
}
