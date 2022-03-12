using System.Text.Json;
using EventStore.Client;
using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.AppendingEvents;

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

public class GettingStateFromEventsTests
{
    private Task<IWriteResult> AppendEvents(EventStoreClient eventStore, string streamName, object[] events,
        CancellationToken ct)
    {
        // TODO: Fill append events logic here.
        return eventStore.AppendToStreamAsync(
            streamName,
            StreamState.Any,
            events.Select(@event =>
                new EventData(
                    Uuid.NewUuid(),
                    @event.GetType().Name,
                    JsonSerializer.SerializeToUtf8Bytes(@event)
                )
            ), cancellationToken: ct);
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public async Task GettingState_ForSequenceOfEvents_ShouldSucceed()
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
            new ShoppingCartOpened(shoppingCartId, clientId),
            new ProductItemAddedToShoppingCart(shoppingCartId, twoPairsOfShoes),
            new ProductItemAddedToShoppingCart(shoppingCartId, tShirt),
            new ProductItemRemovedFromShoppingCart(shoppingCartId, pairOfShoes),
            new ShoppingCartConfirmed(shoppingCartId, DateTime.UtcNow),
            new ShoppingCartCancelled(shoppingCartId, DateTime.UtcNow)
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
