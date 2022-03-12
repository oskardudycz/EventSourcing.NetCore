using FluentAssertions;
using IntroductionToEventSourcing.AppendingEvents.Tools;
using Marten;
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


public class GettingStateFromEventsTests
{
    private static Task AppendEvents(IDocumentSession documentSession, Guid streamId, object[] events, CancellationToken ct)
    {
        documentSession.Events.Append(
            streamId,
            events
        );
        return documentSession.SaveChangesAsync(ct);
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
            new ShoppingCartCanceled(shoppingCartId, DateTime.UtcNow)
        };

        var options = new StoreOptions();
        options.Connection("PORT = 5432; HOST = localhost; TIMEOUT = 15; POOLING = True; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'");

        using var documentStore = new DocumentStore(options);
        await using var documentSession = documentStore.LightweightSession();

        documentSession.Listeners.Add(MartenEventsChangesListener.Instance);

        var exception = await Record.ExceptionAsync(async () =>
            await AppendEvents(documentSession, shoppingCartId, events, CancellationToken.None)
        );
        exception.Should().BeNull();
        MartenEventsChangesListener.Instance.AppendedEventsCount.Should().Be(events.Length);
    }
}
