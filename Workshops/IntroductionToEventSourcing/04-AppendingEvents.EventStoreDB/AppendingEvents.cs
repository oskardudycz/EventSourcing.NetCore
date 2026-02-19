using EventStore.Client;
using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.AppendingEvents;
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

    // This won't allow external inheritance
    private ShoppingCartEvent(){}
}

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

public class GettingStateFromEventsTests
{
    // TODO: Fill append events logic here.
    private Task<IWriteResult> AppendEvents(EventStoreClient eventStore, string streamName, object[] events,
        CancellationToken ct) =>
        throw new NotImplementedException();

    [Fact]
    [Trait("Category", "SkipCI")]
    public async Task AppendingEvents_ForSequenceOfEvents_ShouldSucceed()
    {
        var shoppingCartId = Guid.CreateVersion7();
        var clientId = Guid.CreateVersion7();
        var shoesId = Guid.CreateVersion7();
        var tShirtId = Guid.CreateVersion7();
        var twoPairsOfShoes = new PricedProductItem(new ProductItem(shoesId, 2), 100);
        var pairOfShoes = new PricedProductItem(new ProductItem(shoesId, 1), 100);
        var tShirt = new PricedProductItem(new ProductItem(tShirtId, 1), 50);

        var events = new object[]
        {
            new ShoppingCartOpened(shoppingCartId, clientId),
            new ProductItemAddedToShoppingCart(shoppingCartId, twoPairsOfShoes),
            new ProductItemAddedToShoppingCart(shoppingCartId, tShirt),
            new ProductItemRemovedFromShoppingCart(shoppingCartId, pairOfShoes),
            new ShoppingCartConfirmed(shoppingCartId, DateTime.UtcNow),
            new ShoppingCartCanceled(shoppingCartId, DateTime.UtcNow)
        };

        await using var eventStore =
            new EventStoreClient(EventStoreClientSettings.Create("esdb://localhost:2113?tls=false"));

        var streamName = $"shopping_cart-{shoppingCartId}";

        var appendedEvents = 0ul;
        var exception = await Record.ExceptionAsync(async () =>
        {
            var result = await AppendEvents(eventStore, streamName, events, CancellationToken.None);
            appendedEvents = result.NextExpectedStreamRevision;
        });

        exception.Should().BeNull();
        appendedEvents.Should().Be((ulong)events.Length - 1);
    }
}
