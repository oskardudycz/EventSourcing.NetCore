using Shouldly;
using IntroductionToEventSourcing.BusinessLogic.Tools;
using Xunit;

namespace IntroductionToEventSourcing.BusinessLogic.Mixed;

using static ShoppingCartEvent;

public static class ShoppingCartExtensions
{
    public static ShoppingCart GetShoppingCart(this EventStore eventStore, Guid shoppingCartId) =>
        eventStore.ReadStream<ShoppingCartEvent>(shoppingCartId)
            .Aggregate(ShoppingCart.Initial(), (shoppingCart, @event) =>
            {
                shoppingCart.Evolve(@event);
                return shoppingCart;
            });
}

public class BusinessLogicTests
{
    [Fact]
    public void RunningSequenceOfBusinessLogic_ShouldGenerateSequenceOfEvents()
    {
        var shoppingCartId = Guid.CreateVersion7();
        var clientId = Guid.CreateVersion7();
        var shoesId = Guid.CreateVersion7();
        var tShirtId = Guid.CreateVersion7();
        var twoPairsOfShoes = new ProductItem { ProductId = shoesId, Quantity = 2 };
        var pairOfShoes = new ProductItem { ProductId = shoesId, Quantity = 1 };
        var tShirt = new ProductItem { ProductId = tShirtId, Quantity = 1 };

        var shoesPrice = 100;
        var tShirtPrice = 50;

        var pricedPairOfShoes = new PricedProductItem { ProductId = shoesId, Quantity = 1, UnitPrice = shoesPrice };
        var pricedTShirt = new PricedProductItem { ProductId = tShirtId, Quantity = 1, UnitPrice = tShirtPrice };

        var eventStore = new EventStore();

        // Open
        var (opened,_) = ShoppingCart.Open(shoppingCartId, clientId);
        eventStore.AppendToStream(shoppingCartId, [opened]);

        // Add Two Pair of Shoes
        var shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
        ShoppingCartEvent result = shoppingCart.AddProduct(
            FakeProductPriceCalculator.Returning(shoesPrice),
            twoPairsOfShoes
        );
        eventStore.AppendToStream(shoppingCartId, [result]);

        // Add T-Shirt
        shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
        result = shoppingCart.AddProduct(
            FakeProductPriceCalculator.Returning(tShirtPrice),
            tShirt
        );
        eventStore.AppendToStream(shoppingCartId, [result]);

        // Remove a pair of shoes
        shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
        result = shoppingCart.RemoveProduct(pricedPairOfShoes);
        eventStore.AppendToStream(shoppingCartId, [result]);

        // Confirm
        shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
        result = shoppingCart.Confirm();
        eventStore.AppendToStream(shoppingCartId, [result]);

        // Try Cancel
        var exception = Record.Exception(() =>
        {
            shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
            result = shoppingCart.Cancel();
            eventStore.AppendToStream(shoppingCartId, [result]);
        });
        exception.ShouldBeOfType<InvalidOperationException>();

        shoppingCart = eventStore.GetShoppingCart(shoppingCartId);

        shoppingCart.Id.ShouldBe(shoppingCartId);
        shoppingCart.ClientId.ShouldBe(clientId);
        shoppingCart.ProductItems.Count().ShouldBe(2);
        shoppingCart.Status.ShouldBe(ShoppingCartStatus.Confirmed);

        shoppingCart.ProductItems[0].ShouldBeEquivalentTo(pricedPairOfShoes);
        shoppingCart.ProductItems[1].ShouldBeEquivalentTo(pricedTShirt);

        var events = eventStore.ReadStream<ShoppingCartEvent>(shoppingCartId);
        events.Count().ShouldBe(5);
        events[0].ShouldBeOfType<ShoppingCartOpened>();
        events[1].ShouldBeOfType<ProductItemAddedToShoppingCart>();
        events[2].ShouldBeOfType<ProductItemAddedToShoppingCart>();
        events[3].ShouldBeOfType<ProductItemRemovedFromShoppingCart>();
        events[4].ShouldBeOfType<ShoppingCartConfirmed>();
    }
}
