using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Immutable.Solution2;

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
public record PricedProductItem(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
);

// ENTITY
public record ShoppingCart(
    Guid Id,
    Guid ClientId,
    ShoppingCartStatus Status,
    PricedProductItem[] ProductItems,
    DateTime? ConfirmedAt = null,
    DateTime? CanceledAt = null
)
{
    public static ShoppingCart Default() =>
        new (default, default, default, Array.Empty<PricedProductItem>());

    public static ShoppingCart When(ShoppingCart shoppingCart, object @event)
    {
        return @event switch
        {
            ShoppingCartOpened(var shoppingCartId, var clientId) =>
                shoppingCart with
                {
                    Id = shoppingCartId,
                    ClientId = clientId,
                    Status = ShoppingCartStatus.Pending
                },
            ProductItemAddedToShoppingCart(_, var pricedProductItem) =>
                shoppingCart with
                {
                    ProductItems = shoppingCart.ProductItems
                        .Union(new [] { pricedProductItem })
                        .GroupBy(pi => pi.ProductId)
                        .Select(group => group.Count() == 1?
                            group.First()
                            : new PricedProductItem(
                                group.Key,
                                group.Sum(pi => pi.Quantity),
                                group.First().UnitPrice
                            )
                        )
                        .ToArray()
                },
            ProductItemRemovedFromShoppingCart(_, var pricedProductItem) =>
                shoppingCart with
                {
                    ProductItems = shoppingCart.ProductItems
                        .Select(pi => pi.ProductId == pricedProductItem.ProductId?
                            new PricedProductItem(
                                pi.ProductId,
                                pi.Quantity - pricedProductItem.Quantity,
                                pi.UnitPrice
                            )
                            :pi
                        )
                        .Where(pi => pi.Quantity > 0)
                        .ToArray()
                },
            ShoppingCartConfirmed(_, var confirmedAt) =>
                shoppingCart with
                {
                    Status = ShoppingCartStatus.Confirmed,
                    ConfirmedAt = confirmedAt
                },
            ShoppingCartCanceled(_, var canceledAt) =>
                shoppingCart with
                {
                    Status = ShoppingCartStatus.Canceled,
                    CanceledAt = canceledAt
                },
            _ => shoppingCart
        };
    }
}

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Canceled = 4
}

public class GettingStateFromEventsTests
{
    /// <summary>
    /// Solution 1 - Fully immutable and functional with linq Aggregate method
    /// </summary>
    /// <param name="events"></param>
    /// <returns></returns>
    private static ShoppingCart GetShoppingCart(IEnumerable<object> events) =>
        events.Aggregate(ShoppingCart.Default(), ShoppingCart.When);

    [Fact]
    [Trait("Category", "SkipCI")]
    public void GettingState_ForSequenceOfEvents_ShouldSucceed()
    {
        var shoppingCartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var shoesId = Guid.NewGuid();
        var tShirtId = Guid.NewGuid();
        var twoPairsOfShoes = new PricedProductItem(shoesId, 2, 100);
        var pairOfShoes = new PricedProductItem(shoesId, 1, 100);
        var tShirt = new PricedProductItem(tShirtId, 1, 50);

        var events = new object[]
        {
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

        shoppingCart.ProductItems[0].Should().Be(pairOfShoes);
        shoppingCart.ProductItems[1].Should().Be(tShirt);
    }
}
