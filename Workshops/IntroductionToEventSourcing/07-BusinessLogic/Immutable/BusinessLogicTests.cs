using Shouldly;
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
        var shoppingCartId = Guid.CreateVersion7();
        var clientId = Guid.CreateVersion7();
        var shoesId = Guid.CreateVersion7();
        var tShirtId = Guid.CreateVersion7();
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
        exception.ShouldBeOfType<InvalidOperationException>();

        shoppingCart = eventStore.GetShoppingCart(shoppingCartId);

        shoppingCart.Id.ShouldBe(shoppingCartId);
        shoppingCart.ClientId.ShouldBe(clientId);
        shoppingCart.ProductItems.Count().ShouldBe(2);
        shoppingCart.Status.ShouldBe(ShoppingCartStatus.Confirmed);

        shoppingCart.ProductItems[0].ShouldBe(pricedPairOfShoes);
        shoppingCart.ProductItems[1].ShouldBe(pricedTShirt);

        var events = eventStore.ReadStream<ShoppingCartEvent>(shoppingCartId);
        events.Count().ShouldBe(5);
        events[0].ShouldBeOfType<ShoppingCartOpened>();
        events[1].ShouldBeOfType<ProductItemAddedToShoppingCart>();
        events[2].ShouldBeOfType<ProductItemAddedToShoppingCart>();
        events[3].ShouldBeOfType<ProductItemRemovedFromShoppingCart>();
        events[4].ShouldBeOfType<ShoppingCartConfirmed>();
    }
}
