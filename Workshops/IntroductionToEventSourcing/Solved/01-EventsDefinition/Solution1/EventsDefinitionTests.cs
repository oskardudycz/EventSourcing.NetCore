using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.EventsDefinition.Solution1;

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

public record ImmutablePricedProductItem(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
)
{
    public decimal TotalPrice => Quantity * UnitPrice;
}

// ENTITY

// regular one
public class ShoppingCart
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public ShoppingCartStatus Status { get; set; }
    public IList<PricedProductItem> ProductItems { get; set; } = new List<PricedProductItem>();
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CanceledAt { get; set; }
}

// immutable one
public record ImmutableShoppingCart(
    Guid Id,
    Guid ClientId,
    ShoppingCartStatus Status,
    PricedProductItem[] ProductItems,
    DateTime? ConfirmedAt = null,
    DateTime? CanceledAt = null
);

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Canceled = 4
}

public class EventsDefinitionTests
{
    [Fact]
    public void AllEventTypes_ShouldBeDefined()
    {
        var shoppingCartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var pairOfShoes = new PricedProductItem
        {
            ProductId = Guid.NewGuid(),
            Quantity = 1,
            UnitPrice = 100
        };

        var events = new object[]
        {
            new ShoppingCartOpened(shoppingCartId, clientId),
            new ProductItemAddedToShoppingCart(shoppingCartId, pairOfShoes),
            new ProductItemRemovedFromShoppingCart(shoppingCartId, pairOfShoes),
            new ShoppingCartConfirmed(shoppingCartId, DateTime.UtcNow),
            new ShoppingCartCanceled(shoppingCartId, DateTime.UtcNow)
        };

        const int expectedEventTypesCount = 5;
        events.Should().HaveCount(expectedEventTypesCount);
        events.GroupBy(e => e.GetType()).Should().HaveCount(expectedEventTypesCount);
    }
}
