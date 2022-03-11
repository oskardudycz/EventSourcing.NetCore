using Carts.ShoppingCarts;
using Carts.ShoppingCarts.OpeningCart;
using Carts.Tests.Extensions.Reservations;
using Carts.Tests.Stubs.Events;
using Carts.Tests.Stubs.Repositories;
using FluentAssertions;
using Xunit;

namespace Carts.Tests.Carts.OpeningCart;

public class OpenCartCommandHandlerTests
{
    [Fact]
    public async Task ForInitCardCommand_ShouldAddNewCart()
    {
        // Given
        var repository = new FakeRepository<ShoppingCart>();
        var scope = new DummyEventStoreDBAppendScope();

        var commandHandler = new HandleOpenCart(
            repository,
            scope
        );

        var command = OpenShoppingCart.Create(Guid.NewGuid(), Guid.NewGuid());

        // When
        await commandHandler.Handle(command, CancellationToken.None);

        //Then
        repository.Aggregates.Should().HaveCount(1);

        var cart = repository.Aggregates.Values.Single();

        cart
            .IsOpenedCartWith(
                command.CartId,
                command.ClientId
            )
            .HasCartOpenedEventWith(
                command.CartId,
                command.ClientId
            );
    }
}
