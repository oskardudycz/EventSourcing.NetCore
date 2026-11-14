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
        var cartId = Guid.CreateVersion7();
        var clientId = Guid.CreateVersion7();

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
