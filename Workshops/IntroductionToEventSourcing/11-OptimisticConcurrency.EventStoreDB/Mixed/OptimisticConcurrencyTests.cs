using EventStore.Client;
using FluentAssertions;
using IntroductionToEventSourcing.OptimisticConcurrency.Tools;
using Xunit;

namespace IntroductionToEventSourcing.OptimisticConcurrency.Mixed;

// EVENTS
public abstract record ShoppingCartEvent
{
    public record ShoppingCartOpened(
        Guid ShoppingCartId,
        Guid ClientId
    ): ShoppingCartEvent;

    public record ProductItemAddedToShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem
    ): ShoppingCartEvent;

    public record ProductItemRemovedFromShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem
    ): ShoppingCartEvent;

    public record ShoppingCartConfirmed(
        Guid ShoppingCartId,
        DateTime ConfirmedAt
    ): ShoppingCartEvent;

    public record ShoppingCartCanceled(
        Guid ShoppingCartId,
        DateTime CanceledAt
    ): ShoppingCartEvent;

    // This won't allow external inheritance
    private ShoppingCartEvent(){}
}

// VALUE OBJECTS
public record ProductItem(
    Guid ProductId,
    int Quantity
);

public record PricedProductItem(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
);

public static class ShoppingCartExtensions
{
    public static ShoppingCart GetShoppingCart(this IEnumerable<object> events)
    {
        var shoppingCart = (ShoppingCart)Activator.CreateInstance(typeof(ShoppingCart), true)!;

        foreach (var @event in events)
        {
            shoppingCart.When(@event);
        }

        return shoppingCart;
    }
}

public class OptimisticConcurrencyTests: EventStoreDBTest
{
    [Fact]
    [Trait("Category", "SkipCI")]
    public async Task GettingState_ForSequenceOfEvents_ShouldSucceed()
    {
        var shoppingCartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var shoesId = Guid.NewGuid();
        var tShirtId = Guid.NewGuid();

        var twoPairsOfShoes = new ProductItem(shoesId, 2);
        var tShirt = new ProductItem(tShirtId, 1);

        var shoesPrice = 100;
        var tShirtPrice = 50;

        // Open
        await EventStore.Add<ShoppingCart, OpenShoppingCart>(
            command => ShoppingCart.StreamName(command.ShoppingCartId),
            command =>
                ShoppingCart.Open(command.ShoppingCartId, command.ClientId).Event,
            OpenShoppingCart.From(shoppingCartId, clientId),
            CancellationToken.None
        );

        // Try to open again
        // Should fail as stream was already created
        var exception = await Record.ExceptionAsync(async () =>
            {
                await EventStore.Add<ShoppingCart, OpenShoppingCart>(
                    command => ShoppingCart.StreamName(command.ShoppingCartId),
                    command =>
                        ShoppingCart.Open(command.ShoppingCartId, command.ClientId).Event,
                    OpenShoppingCart.From(shoppingCartId, clientId),
                    CancellationToken.None
                );
            }
        );
        exception.Should().BeOfType<WrongExpectedVersionException>();

        // Add two pairs of shoes
        await EventStore.GetAndUpdate<ShoppingCart, AddProductItemToShoppingCart>(
            command => ShoppingCart.StreamName(command.ShoppingCartId),
            (command, shoppingCart) =>
                shoppingCart.AddProduct(FakeProductPriceCalculator.Returning(shoesPrice), command.ProductItem),
            AddProductItemToShoppingCart.From(shoppingCartId, twoPairsOfShoes),
            0,
            CancellationToken.None
        );

        // Add T-Shirt
        // Should fail because of sending the same expected version as previous call
        exception = await Record.ExceptionAsync(async () =>
            {
                await EventStore.GetAndUpdate<ShoppingCart, AddProductItemToShoppingCart>(
                    command => ShoppingCart.StreamName(command.ShoppingCartId),
                    (command, shoppingCart) =>
                        shoppingCart.AddProduct(FakeProductPriceCalculator.Returning(tShirtPrice), command.ProductItem),
                    AddProductItemToShoppingCart.From(shoppingCartId, tShirt),
                    0,
                    CancellationToken.None
                );
            }
        );
        exception.Should().BeOfType<WrongExpectedVersionException>();

        var shoppingCart = await EventStore.Get<ShoppingCart>(ShoppingCart.StreamName(shoppingCartId), CancellationToken.None);

        shoppingCart.Id.Should().Be(shoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.ProductItems.Should().HaveCount(1);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Pending);

        shoppingCart.ProductItems[0].ProductId.Should().Be(shoesId);
        shoppingCart.ProductItems[0].Quantity.Should().Be(twoPairsOfShoes.Quantity);
        shoppingCart.ProductItems[0].UnitPrice.Should().Be(shoesPrice);
    }
}
