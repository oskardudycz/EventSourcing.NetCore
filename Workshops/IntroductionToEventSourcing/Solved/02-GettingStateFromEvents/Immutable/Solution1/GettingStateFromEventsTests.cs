using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Immutable.Solution1;
using static ShoppingCartEvent;

// EVENTS
public abstract record ShoppingCartEvent
{
    public record ShoppingCartOpened(
        Guid ShoppingCartId,
        Guid ClientId
    ): ShoppingCartEvent;

    public record ProductItemAddedToShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem
    ): ShoppingCartEvent;

    public record ProductItemRemovedFromShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem
    ): ShoppingCartEvent;

    public record ShoppingCartConfirmed(
        Guid ShoppingCartId,
        DateTime ConfirmedAt
    ): ShoppingCartEvent;

    public record ShoppingCartCanceled(
        Guid ShoppingCartId,
        DateTime CanceledAt
    ): ShoppingCartEvent;

    // This won't allow
    private ShoppingCartEvent(){}
}

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
);

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Canceled = 4
}

public class GettingStateFromEventsTests
{
    /// <summary>
    /// Solution 1 - Immutable entity using foreach with switch pattern matching
    /// </summary>
    /// <param name="events"></param>
    /// <returns></returns>
    private static ShoppingCart GetShoppingCart(IEnumerable<ShoppingCartEvent> events)
    {
        ShoppingCart shoppingCart = null!;

        foreach (var @event in events)
        {
            switch (@event)
            {
                case ShoppingCartOpened opened:
                    shoppingCart = new ShoppingCart(
                        opened.ShoppingCartId,
                        opened.ClientId,
                        ShoppingCartStatus.Pending,
                        []
                    );
                    break;
                case ProductItemAddedToShoppingCart productItemAdded:
                    shoppingCart = shoppingCart with
                    {
                        ProductItems = shoppingCart.ProductItems
                            .Concat(new []{ productItemAdded.ProductItem })
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
                    };
                    break;
                case ProductItemRemovedFromShoppingCart productItemRemoved:
                    shoppingCart = shoppingCart with
                    {
                        ProductItems = shoppingCart.ProductItems
                            .Select(pi => pi.ProductId == productItemRemoved.ProductItem.ProductId?
                                pi with { Quantity = pi.Quantity - productItemRemoved.ProductItem.Quantity }
                                :pi
                            )
                            .Where(pi => pi.Quantity > 0)
                            .ToArray()
                    };
                    break;
                case ShoppingCartConfirmed confirmed:
                    shoppingCart = shoppingCart with
                    {
                        Status = ShoppingCartStatus.Confirmed,
                        ConfirmedAt = confirmed.ConfirmedAt
                    };
                    break;
                case ShoppingCartCanceled canceled:
                    shoppingCart = shoppingCart with
                    {
                        Status = ShoppingCartStatus.Canceled,
                        CanceledAt = canceled.CanceledAt
                    };
                    break;
            }
        }

        return shoppingCart;
    }

    [Fact]
    public void GettingState_ForSequenceOfEvents_ShouldSucceed()
    {
        var shoppingCartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var shoesId = Guid.NewGuid();
        var tShirtId = Guid.NewGuid();
        var twoPairsOfShoes = new PricedProductItem(shoesId, 2, 100);
        var pairOfShoes = new PricedProductItem(shoesId, 1, 100);
        var tShirt = new PricedProductItem(tShirtId, 1, 50);

        var events = new ShoppingCartEvent[]
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
