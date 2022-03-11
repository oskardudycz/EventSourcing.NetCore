using Carts.ShoppingCarts;
using Carts.Tests.Extensions.Reservations;
using Xunit;

namespace Carts.Tests.Carts.OpeningCart;

public class OpenCartTests
{
    [Fact]
    public void ForValidParams_ShouldCreateCartWithPendingStatus()
    {
        // Given
        var cartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        // When
        var cart = ShoppingCart.Open(
            cartId,
            clientId
        );

        // Then

        cart
            .IsOpenedCartWith(
                cartId,
                clientId
            )
            .HasCartOpenedEventWith(
                cartId,
                clientId
            );
    }
}
