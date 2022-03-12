using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Solution2;

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
    PricedProductItem[] ProductItems,
    DateTime? ConfirmedAt = null,
    DateTime? CanceledAt = null
);

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Canceled = 3
}

public class GettingStateFromEventsTests
{
    /// <summary>
    /// Solution 1 - Immutable entity using foreach with switch pattern matching
    /// </summary>
    /// <param name="events"></param>
    /// <returns></returns>
    private static ShoppingCart GetShoppingCart(IEnumerable<object> events)
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
                        Array.Empty<PricedProductItem>()
                    );
                    break;
                case ProductItemAddedToShoppingCart productItemAdded:
                    shoppingCart = shoppingCart with
                    {
                        ProductItems = shoppingCart.ProductItems
                            .Union(new []{ productItemAdded.ProductItem })
                            .GroupBy(pi => pi.ProductId)
                            .Select(group => group.Count() == 1?
                                group.First()
                                : new PricedProductItem(
                                    new ProductItem(productItemAdded.ProductItem.ProductId, group.Sum(pi => pi.Quantity)),
                                    productItemAdded.ProductItem.UnitPrice
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
                                new PricedProductItem(
                                    new ProductItem(pi.ProductId, pi.Quantity - productItemRemoved.ProductItem.Quantity),
                                    pi.UnitPrice
                                )
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
    [Trait("Category", "SkipCI")]
    public void GettingState_ForSequenceOfEvents_ShouldSucceed()
    {
        var shoppingCartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var shoesId = Guid.NewGuid();
        var tShirtId = Guid.NewGuid();
        var twoPairsOfShoes = new PricedProductItem(new ProductItem(shoesId, 2), 100);
        var pairOfShoes = new PricedProductItem(new ProductItem(shoesId, 1), 100);
        var tShirt = new PricedProductItem(new ProductItem(tShirtId, 1), 50);

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

        shoppingCart.ProductItems[0].Should().Be(pairOfShoes);
        shoppingCart.ProductItems[1].Should().Be(tShirt);
    }
}
