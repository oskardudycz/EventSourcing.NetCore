using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.EventsDefinition;

// 1. Define your events and entity here

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

public record ShoppingCartCancelled(
    Guid ShoppingCartId,
    DateTime CancelledAt
);

// VALUE OBJECTS
public record PricedProductItem(
    ProductItem ProductItem,
    decimal UnitPrice
)
{
    public Guid ProductId => ProductItem.ProductId;
    public int Quantity => ProductItem.Quantity;

    public decimal TotalPrice => Quantity * UnitPrice;
}

public record ProductItem(
    Guid ProductId,
    int Quantity
);

// ENTITY

public record ShoppingCart(
    Guid Id,
    Guid ClientId,
    ShoppingCartStatus Status,
    PricedProductItem ProductItems,
    DateTime? ConfirmedAt = null
);

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Cancelled = 3
}

public class EventsDefinitionTests
{

    [Fact]
    [Trait("Category", "SkipCI")]
    public void AllEventTypes_ShouldBeDefined()
    {
        var shoppingCartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var pairOfShoes = new PricedProductItem(new ProductItem(Guid.NewGuid(), 1), 100);

        var events = new object[]
        {
            // 2. Put your sample events here
            new ShoppingCartOpened(shoppingCartId, clientId),
            new ProductItemAddedToShoppingCart(shoppingCartId, pairOfShoes),
            new ProductItemRemovedFromShoppingCart(shoppingCartId, pairOfShoes),
            new ShoppingCartConfirmed(shoppingCartId, DateTime.UtcNow),
            new ShoppingCartCancelled(shoppingCartId, DateTime.UtcNow)
        };

        const int expectedEventTypesCount = 5;
        events.Should().HaveCount(expectedEventTypesCount);
        events.GroupBy(e => e.GetType()).Should().HaveCount(expectedEventTypesCount);
    }
}
