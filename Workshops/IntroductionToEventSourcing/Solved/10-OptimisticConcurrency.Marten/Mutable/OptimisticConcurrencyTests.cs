using FluentAssertions;
using IntroductionToEventSourcing.OptimisticConcurrency.Tools;
using Marten.Exceptions;
using Xunit;

namespace IntroductionToEventSourcing.OptimisticConcurrency.Mutable;

// EVENTS
public record ShoppingCartOpened(
    Guid ShoppingCartId,
    Guid ClientId
);

public record ProductItemAddedToShoppingCart(
    Guid ShoppingCartId,
    PricedProductItem ProductItem
);

public record ProductItemRemovedFromShoppingCart(
    Guid ShoppingCartId,
    PricedProductItem ProductItem
);

public record ShoppingCartConfirmed(
    Guid ShoppingCartId,
    DateTime ConfirmedAt
);

public record ShoppingCartCanceled(
    Guid ShoppingCartId,
    DateTime CanceledAt
);

// VALUE OBJECTS
public class PricedProductItem
{
    public Guid ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public class ProductItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class OptimisticConcurrencyTests: MartenTest
{
    [Fact]
    public async Task GettingState_ForSequenceOfEvents_ShouldSucceed()
    {
        var shoppingCartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var shoesId = Guid.NewGuid();
        var tShirtId = Guid.NewGuid();

        var twoPairsOfShoes = new ProductItem { ProductId = shoesId, Quantity = 2 };
        var tShirt = new ProductItem { ProductId = tShirtId, Quantity = 1 };

        var shoesPrice = 100;
        var tShirtPrice = 50;

        // Open
        await DocumentSession.Add(
            command => command.ShoppingCartId,
            command =>
                ShoppingCart.Open(command.ShoppingCartId, command.ClientId),
            OpenShoppingCart.From(shoppingCartId, clientId),
            CancellationToken.None
        );

        // Try to open again
        // Should fail as stream was already created
        var exception = await Record.ExceptionAsync(async () =>
            {
                await DocumentSession.Add(
                    command => command.ShoppingCartId,
                    command =>
                        ShoppingCart.Open(command.ShoppingCartId, command.ClientId),
                    OpenShoppingCart.From(shoppingCartId, clientId),
                    CancellationToken.None
                );
            }
        );
        exception.Should().BeOfType<ExistingStreamIdCollisionException>();
        ReOpenSession();

        // Add two pairs of shoes
        await DocumentSession.GetAndUpdate<ShoppingCart, AddProductItemToShoppingCart>(
            command => command.ShoppingCartId,
            (command, shoppingCart) =>
                shoppingCart.AddProduct(FakeProductPriceCalculator.Returning(shoesPrice), command.ProductItem),
            AddProductItemToShoppingCart.From(shoppingCartId, twoPairsOfShoes),
            1,
            CancellationToken.None
        );

        // Add T-Shirt
        // Should fail because of sending the same expected version as previous call
        exception = await Record.ExceptionAsync(async () =>
            {
                await DocumentSession.GetAndUpdate<ShoppingCart, AddProductItemToShoppingCart>(
                    command => command.ShoppingCartId,
                    (command, shoppingCart) =>
                        shoppingCart.AddProduct(FakeProductPriceCalculator.Returning(tShirtPrice), command.ProductItem),
                    AddProductItemToShoppingCart.From(shoppingCartId, tShirt),
                    1,
                    CancellationToken.None
                );
            }
        );
        exception.Should().BeOfType<EventStreamUnexpectedMaxEventIdException>();

        var shoppingCart = await DocumentSession.Get<ShoppingCart>(shoppingCartId, CancellationToken.None);

        shoppingCart.Id.Should().Be(shoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.ProductItems.Should().HaveCount(1);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Pending);

        shoppingCart.ProductItems[0].ProductId.Should().Be(shoesId);
        shoppingCart.ProductItems[0].Quantity.Should().Be(twoPairsOfShoes.Quantity);
        shoppingCart.ProductItems[0].UnitPrice.Should().Be(shoesPrice);
    }
}
