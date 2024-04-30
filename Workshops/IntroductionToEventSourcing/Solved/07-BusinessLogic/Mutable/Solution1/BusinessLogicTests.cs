using FluentAssertions;
using IntroductionToEventSourcing.BusinessLogic.Tools;
using Xunit;

namespace IntroductionToEventSourcing.BusinessLogic.Mutable.Solution1;

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
        var shoppingCartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var shoesId = Guid.NewGuid();
        var tShirtId = Guid.NewGuid();
        var twoPairsOfShoes = new ProductItem { ProductId = shoesId, Quantity = 2 };
        var pairOfShoes = new ProductItem { ProductId = shoesId, Quantity = 1 };
        var tShirt = new ProductItem { ProductId = tShirtId, Quantity = 1 };

        var shoesPrice = 100;
        var tShirtPrice = 50;

        var pricedPairOfShoes = new PricedProductItem { ProductId = shoesId, Quantity = 1, UnitPrice = shoesPrice };
        var pricedTShirt = new PricedProductItem { ProductId = tShirtId, Quantity = 1, UnitPrice = tShirtPrice };

        var eventStore = new EventStore();

        // Open
        var shoppingCart = ShoppingCart.Open(shoppingCartId, clientId);
        eventStore.AppendToStream(shoppingCartId, shoppingCart.DequeueUncommittedEvents());

        // Add Two Pair of Shoes
        shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
        shoppingCart.AddProduct(
            FakeProductPriceCalculator.Returning(shoesPrice),
            twoPairsOfShoes
        );
        eventStore.AppendToStream(shoppingCartId, shoppingCart.DequeueUncommittedEvents());

        // Add T-Shirt
        shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
        shoppingCart.AddProduct(
            FakeProductPriceCalculator.Returning(tShirtPrice),
            tShirt
        );
        eventStore.AppendToStream(shoppingCartId, shoppingCart.DequeueUncommittedEvents());

        // Remove a pair of shoes
        shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
        shoppingCart.RemoveProduct(pricedPairOfShoes);
        eventStore.AppendToStream(shoppingCartId, shoppingCart.DequeueUncommittedEvents());

        // Confirm
        shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
        shoppingCart.Confirm();
        eventStore.AppendToStream(shoppingCartId, shoppingCart.DequeueUncommittedEvents());

        // Try Cancel
        var exception = Record.Exception(() =>
        {
            shoppingCart = eventStore.GetShoppingCart(shoppingCartId);
            shoppingCart.Cancel();
            eventStore.AppendToStream(shoppingCartId, shoppingCart.DequeueUncommittedEvents());
        });
        exception.Should().BeOfType<InvalidOperationException>();

        shoppingCart = eventStore.GetShoppingCart(shoppingCartId);

        shoppingCart.Id.Should().Be(shoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.ProductItems.Should().HaveCount(2);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);

        shoppingCart.ProductItems[0].Should().BeEquivalentTo(pricedPairOfShoes);
        shoppingCart.ProductItems[1].Should().BeEquivalentTo(pricedTShirt);

        var events = eventStore.ReadStream<ShoppingCartEvent>(shoppingCartId);
        events.Should().HaveCount(5);
        events[0].Should().BeOfType<ShoppingCartOpened>();
        events[1].Should().BeOfType<ProductItemAddedToShoppingCart>();
        events[2].Should().BeOfType<ProductItemAddedToShoppingCart>();
        events[3].Should().BeOfType<ProductItemRemovedFromShoppingCart>();
        events[4].Should().BeOfType<ShoppingCartConfirmed>();
    }
}
