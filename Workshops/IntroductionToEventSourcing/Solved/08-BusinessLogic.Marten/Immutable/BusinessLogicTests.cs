using FluentAssertions;
using IntroductionToEventSourcing.BusinessLogic.Tools;
using Xunit;

namespace IntroductionToEventSourcing.BusinessLogic.Immutable;

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

// Business logic

public class BusinessLogicTests: MartenTest
{
    [Fact]
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

        await DocumentSession.Add<ShoppingCart, OpenShoppingCart>(
            command => command.ShoppingCartId,
            OpenShoppingCart.Handle,
            OpenShoppingCart.From(shoppingCartId, clientId),
            CancellationToken.None
        );

        // Add two pairs of shoes
        await DocumentSession.GetAndUpdate<ShoppingCart, AddProductItemToShoppingCart>(
            command => command.ShoppingCartId,
            (command, shoppingCart) =>
                AddProductItemToShoppingCart.Handle(FakeProductPriceCalculator.Returning(shoesPrice), command, shoppingCart),
            AddProductItemToShoppingCart.From(shoppingCartId, twoPairsOfShoes),
            CancellationToken.None
        );

        // Add T-Shirt
        await DocumentSession.GetAndUpdate<ShoppingCart, AddProductItemToShoppingCart>(
            command => command.ShoppingCartId,
            (command, shoppingCart) =>
                AddProductItemToShoppingCart.Handle(FakeProductPriceCalculator.Returning(tShirtPrice), command, shoppingCart),
            AddProductItemToShoppingCart.From(shoppingCartId, tShirt),
            CancellationToken.None
        );

        // Remove pair of shoes
        await DocumentSession.GetAndUpdate<ShoppingCart, RemoveProductItemFromShoppingCart>(
            command => command.ShoppingCartId,
            RemoveProductItemFromShoppingCart.Handle,
            RemoveProductItemFromShoppingCart.From(shoppingCartId, pricedPairOfShoes),
            CancellationToken.None
        );

        // Confirm
        await DocumentSession.GetAndUpdate<ShoppingCart, ConfirmShoppingCart>(
            command => command.ShoppingCartId,
            ConfirmShoppingCart.Handle,
            ConfirmShoppingCart.From(shoppingCartId),
            CancellationToken.None
        );

        // Cancel
        var exception = await Record.ExceptionAsync(async () =>
            {
                await DocumentSession.GetAndUpdate<ShoppingCart, CancelShoppingCart>(
                    command => command.ShoppingCartId,
                    CancelShoppingCart.Handle,
                    CancelShoppingCart.From(shoppingCartId),
                    CancellationToken.None
                );
            }
        );
        exception.Should().BeOfType<InvalidOperationException>();

        var shoppingCart = await DocumentSession.Get<ShoppingCart>(shoppingCartId, CancellationToken.None);

        shoppingCart.Id.Should().Be(shoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.ProductItems.Should().HaveCount(2);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);

        shoppingCart.ProductItems[0].Should().Be(pricedPairOfShoes);
        shoppingCart.ProductItems[1].Should().Be(pricedTShirt);
    }
}
