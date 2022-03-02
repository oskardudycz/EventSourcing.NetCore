using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Carts.ShoppingCarts;
using Carts.ShoppingCarts.InitializingCart;
using Carts.Tests.Extensions.Reservations;
using Carts.Tests.Stubs.Repositories;
using Core.Testing;
using FluentAssertions;
using Xunit;

namespace Carts.Tests.Carts.InitializingCart;

public class InitializeCardCommandHandlerTests
{
    [Fact]
    public async Task ForInitCardCommand_ShouldAddNewCart()
    {
        // Given
        var repository = new FakeRepository<ShoppingCart>();

        var commandHandler = new HandleInitializeCart(
            repository
        );

        var command = InitializeShoppingCart.Create(Guid.NewGuid(), Guid.NewGuid());

        // When
        await commandHandler.Handle(command, CancellationToken.None);

        //Then
        repository.Aggregates.Should().HaveCount(1);

        var cart = repository.Aggregates.Values.Single();

        cart
            .IsInitializedCartWith(
                command.CartId,
                command.ClientId
            )
            .HasCartInitializedEventWith(
                command.CartId,
                command.ClientId
            );
    }
}
