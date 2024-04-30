using FluentAssertions;
using IntroductionToEventSourcing.BusinessLogic.Tools;
using Xunit;

namespace IntroductionToEventSourcing.BusinessLogic.Immutable;

using static ShoppingCartEvent;
using static ShoppingCartCommand;

public static class ShoppingCartExtensions
{
    public static ShoppingCart GetShoppingCart(this EventStore eventStore, Guid shoppingCartId) =>
        eventStore.ReadStream<ShoppingCartEvent>(shoppingCartId).Aggregate(ShoppingCart.Default(), ShoppingCart.Evolve);
}

public class BusinessLogicTests
{
    [Fact]
    [Trait("Category", "SkipCI")]
    public void RunningSequenceOfBusinessLogic_ShouldGenerateSequenceOfEvents()
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

        var eventStore = new EventStore();

        // Open
        ShoppingCartEvent result =
            ShoppingCartService.Handle(
                new OpenShoppingCart(shoppingCartId, clientId)
            );
        eventStore.AppendToStream(shoppingCartId, [result]);

        // Add Two Pair of Shoes
        var shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
        result = ShoppingCartService.Handle(
            FakeProductPriceCalculator.Returning(shoesPrice),
            new AddProductItemToShoppingCart(shoppingCartId, twoPairsOfShoes),
            shoppingCart
        );
        eventStore.AppendToStream(shoppingCartId, [result]);

        // Add T-Shirt
        shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
        result = ShoppingCartService.Handle(
            FakeProductPriceCalculator.Returning(tShirtPrice),
            new AddProductItemToShoppingCart(shoppingCartId, tShirt),
            shoppingCart
        );
        eventStore.AppendToStream(shoppingCartId, [result]);

        // Remove a pair of shoes
        shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
        result = ShoppingCartService.Handle(
            new RemoveProductItemFromShoppingCart(shoppingCartId, pricedPairOfShoes),
            shoppingCart
        );
        eventStore.AppendToStream(shoppingCartId, [result]);

        // Confirm
        shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
        result = ShoppingCartService.Handle(
            new ConfirmShoppingCart(shoppingCartId),
            shoppingCart
        );
        eventStore.AppendToStream(shoppingCartId, [result]);

        // Try Cancel
        var exception = Record.Exception(() =>
        {
            shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
            result = ShoppingCartService.Handle(
                new CancelShoppingCart(shoppingCartId),
                shoppingCart
            );
            eventStore.AppendToStream(shoppingCartId, [result]);
        });
        exception.Should().BeOfType<InvalidOperationException>();

        shoppingCart = eventStore.GetShoppingCart(shoppingCartId);

        shoppingCart.Id.Should().Be(shoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.ProductItems.Should().HaveCount(2);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);

        shoppingCart.ProductItems[0].Should().Be(pricedPairOfShoes);
        shoppingCart.ProductItems[1].Should().Be(pricedTShirt);

        var events = eventStore.ReadStream<ShoppingCartEvent>(shoppingCartId);
        events.Should().HaveCount(5);
        events[0].Should().BeOfType<ShoppingCartOpened>();
        events[1].Should().BeOfType<ProductItemAddedToShoppingCart>();
        events[2].Should().BeOfType<ProductItemAddedToShoppingCart>();
        events[3].Should().BeOfType<ProductItemRemovedFromShoppingCart>();
        events[4].Should().BeOfType<ShoppingCartConfirmed>();
    }
}
