using FluentAssertions;
using IntroductionToEventSourcing.BusinessLogic.Tools;
using Xunit;

namespace IntroductionToEventSourcing.BusinessLogic.Mixed;

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

public class BusinessLogicTests: EventStoreDBTest
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
        var pairOfShoes = new ProductItem(shoesId, 1);
        var tShirt = new ProductItem(tShirtId, 1);

        var shoesPrice = 100;
        var tShirtPrice = 50;

        var pricedPairOfShoes = new PricedProductItem(shoesId, 1, shoesPrice);
        var pricedTShirt = new PricedProductItem(tShirtId, 1, tShirtPrice);

        // Open
        await EventStore.Add<ShoppingCart, OpenShoppingCart>(
            command => ShoppingCart.StreamName(command.ShoppingCartId),
            command =>
                ShoppingCart.Open(command.ShoppingCartId, command.ClientId).Event,
            OpenShoppingCart.From(shoppingCartId, clientId),
            CancellationToken.None
        );

        // Add two pairs of shoes
        await EventStore.GetAndUpdate<ShoppingCart, AddProductItemToShoppingCart>(
            command => ShoppingCart.StreamName(command.ShoppingCartId),
            (command, shoppingCart) =>
                shoppingCart.AddProduct(FakeProductPriceCalculator.Returning(shoesPrice), command.ProductItem),
            AddProductItemToShoppingCart.From(shoppingCartId, twoPairsOfShoes),
            CancellationToken.None
        );

        // Add T-Shirt
        await EventStore.GetAndUpdate<ShoppingCart, AddProductItemToShoppingCart>(
            command => ShoppingCart.StreamName(command.ShoppingCartId),
            (command, shoppingCart) =>
                shoppingCart.AddProduct(FakeProductPriceCalculator.Returning(tShirtPrice), command.ProductItem),
            AddProductItemToShoppingCart.From(shoppingCartId, tShirt),
            CancellationToken.None
        );

        // Remove pair of shoes
        await EventStore.GetAndUpdate<ShoppingCart, RemoveProductItemFromShoppingCart>(
            command => ShoppingCart.StreamName(command.ShoppingCartId),
            (command, shoppingCart) =>
                shoppingCart.RemoveProduct(command.ProductItem),
            RemoveProductItemFromShoppingCart.From(shoppingCartId, pricedPairOfShoes),
            CancellationToken.None
        );

        // Confirm
        await EventStore.GetAndUpdate<ShoppingCart, ConfirmShoppingCart>(
            command => ShoppingCart.StreamName(command.ShoppingCartId),
            (_, shoppingCart) =>
                shoppingCart.Confirm(),
            ConfirmShoppingCart.From(shoppingCartId),
            CancellationToken.None
        );

        // Cancel
        var exception = await Record.ExceptionAsync(async () =>
            {
                await EventStore.GetAndUpdate<ShoppingCart, CancelShoppingCart>(
                    command => ShoppingCart.StreamName(command.ShoppingCartId),
                    (_, shoppingCart) =>
                        shoppingCart.Cancel(),
                    CancelShoppingCart.From(shoppingCartId),
                    CancellationToken.None
                );
            }
        );
        exception.Should().BeOfType<InvalidOperationException>();

        var shoppingCart = await EventStore.Get<ShoppingCart>(ShoppingCart.StreamName(shoppingCartId), CancellationToken.None);

        shoppingCart.Id.Should().Be(shoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.ProductItems.Should().HaveCount(2);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);

        shoppingCart.ProductItems[0].ProductId.Should().Be(shoesId);
        shoppingCart.ProductItems[0].Quantity.Should().Be(pricedPairOfShoes.Quantity);
        shoppingCart.ProductItems[0].UnitPrice.Should().Be(pricedPairOfShoes.UnitPrice);

        shoppingCart.ProductItems[1].ProductId.Should().Be(tShirtId);
        shoppingCart.ProductItems[1].Quantity.Should().Be(pricedTShirt.Quantity);
        shoppingCart.ProductItems[1].UnitPrice.Should().Be(pricedTShirt.UnitPrice);
    }
}
