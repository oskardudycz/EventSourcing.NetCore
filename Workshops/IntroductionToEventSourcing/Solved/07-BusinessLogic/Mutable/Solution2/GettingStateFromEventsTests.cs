using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Mutable.Solution2;

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

public class GettingStateFromEventsTests
{
    [Fact]
    [Trait("Category", "SkipCI")]
    public void GettingState_ForSequenceOfEvents_ShouldSucceed()
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

        var events = new List<object>
        {
            // TODO: Fill the events object with results of your business logic
            // to be the same as events below

            // new ShoppingCartOpened(shoppingCartId, clientId),
            // new ProductItemAddedToShoppingCart(shoppingCartId, twoPairsOfShoes),
            // new ProductItemAddedToShoppingCart(shoppingCartId, tShirt),
            // new ProductItemRemovedFromShoppingCart(shoppingCartId, pairOfShoes),
            // new ShoppingCartConfirmed(shoppingCartId, DateTime.UtcNow),
            // new ShoppingCartCanceled(shoppingCartId, DateTime.UtcNow)
        };


        // Open
        var (opened, shoppingCart) = ShoppingCart.Open(shoppingCartId, clientId);
        events.Add(opened);

        // Add two pairs of shoes
        shoppingCart = events.GetShoppingCart();
        events.Add(shoppingCart.AddProduct(FakeProductPriceCalculator.Returning(shoesPrice), twoPairsOfShoes));

        // Add T-Shirt
        shoppingCart = events.GetShoppingCart();
        events.Add(shoppingCart.AddProduct(FakeProductPriceCalculator.Returning(tShirtPrice), tShirt));

        // Remove pair of shoes
        events.Add(shoppingCart.RemoveProduct(pricedPairOfShoes));

        // Confirm
        // Uncomment line below and debug to find bug.
        // shoppingCart = events.GetShoppingCart();
        events.Add(shoppingCart.Confirm());

        // Cancel
        var exception = Record.Exception(() =>
            {
                events.Add(shoppingCart.Cancel());
            }
        );
        exception.Should().BeOfType<InvalidOperationException>();

        // Uncomment line below and debug to find bug.
        // shoppingCart = events.GetShoppingCart();

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
