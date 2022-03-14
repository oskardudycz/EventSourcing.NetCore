using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.BusinessLogic.Mutable.Solution1;

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

public class BusinessLogicTests
{
    [Fact]
    public void GettingState_ForSequenceOfEvents_ShouldSucceed()
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
        var pricedTShirt = new PricedProductItem{ ProductId = tShirtId, Quantity = 1, UnitPrice = tShirtPrice };

        var events = new List<object>();

        // Open
        var shoppingCart = ShoppingCart.Open(shoppingCartId, clientId);
        events.AddRange(shoppingCart.DequeueUncommittedEvents());

        // Add two pairs of shoes
        shoppingCart = events.GetShoppingCart();
        shoppingCart.AddProduct(FakeProductPriceCalculator.Returning(shoesPrice), twoPairsOfShoes);
        events.AddRange(shoppingCart.DequeueUncommittedEvents());

        // Add T-Shirt
        shoppingCart = events.GetShoppingCart();
        shoppingCart.AddProduct(FakeProductPriceCalculator.Returning(tShirtPrice), tShirt);
        events.AddRange(shoppingCart.DequeueUncommittedEvents());

        // Remove pair of shoes
        // Hack alert!
        //
        // See that's why immutability is so cool, as it's predictable
        // As we're sharing objects (e.g. in PricedProductItem in events)
        // then adding them into list and changing it while appending/removing
        // then we can have unpleasant surprises.
        //
        // This will not likely happen if all objects are recreated (e.g. in the web requests)
        // However when it happens then it's tricky to diagnose.
        // Uncomment lines below and debug to find more.
        // shoppingCart = events.GetShoppingCart();
        shoppingCart.RemoveProduct(pricedPairOfShoes);
        events.AddRange(shoppingCart.DequeueUncommittedEvents());

        // Confirm
        // Uncomment line below and debug to find bug.
        // shoppingCart = events.GetShoppingCart();
        shoppingCart.Confirm();
        events.AddRange(shoppingCart.DequeueUncommittedEvents());

        // Cancel
        var exception = Record.Exception(() =>
            {
                // Uncomment line below and debug to find bug.
                // shoppingCart = events.GetShoppingCart();
                shoppingCart.Cancel();
                events.AddRange(shoppingCart.DequeueUncommittedEvents());
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
