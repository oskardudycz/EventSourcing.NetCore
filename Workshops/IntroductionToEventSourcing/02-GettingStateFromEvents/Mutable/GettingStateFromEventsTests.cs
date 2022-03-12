using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Mutable;

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

// ENTITY
public class ShoppingCart
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public ShoppingCartStatus Status { get; set; }
    public IList<PricedProductItem> ProductItems { get; set; } = new List<PricedProductItem>();
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CanceledAt { get; set; }
}

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Canceled = 3
}

public class GettingStateFromEventsTests
{
    private static ShoppingCart GetShoppingCart(IEnumerable<object> events)
    {
        // 1. Add logic here
        throw new NotImplementedException();
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public void GettingState_ForSequenceOfEvents_ShouldSucceed()
    {
        var shoppingCartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var shoesId = Guid.NewGuid();
        var tShirtId = Guid.NewGuid();
        var twoPairsOfShoes =
            new PricedProductItem
            {
                ProductId = shoesId, Quantity = 2, UnitPrice = 100
            };
        var pairOfShoes =
            new PricedProductItem
            {
                ProductId = shoesId, Quantity = 1, UnitPrice = 100
            };
        var tShirt =
            new PricedProductItem
            {
                ProductId = tShirtId, Quantity = 1, UnitPrice = 50
            };

        var events = new object[]
        {
            // 2. Put your sample events here
            new ShoppingCartOpened(shoppingCartId, clientId),
            new ProductItemAddedToShoppingCart(shoppingCartId, twoPairsOfShoes),
            new ProductItemAddedToShoppingCart(shoppingCartId, tShirt),
            new ProductItemRemovedFromShoppingCart(shoppingCartId, pairOfShoes),
            new ShoppingCartConfirmed(shoppingCartId, DateTime.UtcNow),
            new ShoppingCartCanceled(shoppingCartId, DateTime.UtcNow)
        };

        var shoppingCart = GetShoppingCart(events);

        shoppingCart.Id.Should().Be(shoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.ProductItems.Should().HaveCount(2);

        shoppingCart.ProductItems[0].ProductId.Should().Be(shoesId);
        shoppingCart.ProductItems[0].Quantity.Should().Be(pairOfShoes.Quantity);
        shoppingCart.ProductItems[0].UnitPrice.Should().Be(pairOfShoes.UnitPrice);

        shoppingCart.ProductItems[1].ProductId.Should().Be(tShirtId);
        shoppingCart.ProductItems[1].Quantity.Should().Be(tShirt.Quantity);
        shoppingCart.ProductItems[1].UnitPrice.Should().Be(tShirt.UnitPrice);
    }
}
