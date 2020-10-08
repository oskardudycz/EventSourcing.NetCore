using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Carts.Carts;
using Carts.Carts.Commands;
using Carts.Tests.Extensions.Reservations;
using Carts.Tests.Stubs.Products;
using Carts.Tests.Stubs.Storage;
using FluentAssertions;
using Xunit;

namespace Carts.Tests.Carts.CommandHandlers
{
    public class InitCardCommandHandlerTests
    {
        [Fact]
        public async Task ForInitCardCommand_ShouldAddNewCart()
        {
            // Given
            var repository = new FakeRepository<Cart>();
            var productPriceCalculator = new FakeProductPriceCalculator();

            var commandHandler = new CartCommandHandler(
                repository,
                productPriceCalculator
            );

            var command = InitCart.Create(Guid.NewGuid(), Guid.NewGuid());

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
}
