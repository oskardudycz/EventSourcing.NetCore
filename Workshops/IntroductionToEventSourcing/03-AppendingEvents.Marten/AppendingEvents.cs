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
        // TODO: Fill append events logic here.
        throw new NotImplementedException();
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public async Task AppendingEvents_ForSequenceOfEvents_ShouldSucceed()
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

        await using var server = new MysticMind.PostgresEmbed.PgServer("15.3.0");

        await server.StartAsync();
        // using Npgsql to connect the server
        var connectionString =
            $"Server=localhost;Port={server.PgPort};User Id=postgres;Password=test;Database=postgres";

        using var documentStore = DocumentStore.For(options =>
        {
            options.Connection(connectionString);
            options.DatabaseSchemaName = options.Events.DatabaseSchemaName = "IntroductionToEventSourcing";
        });
        await using var documentSession = documentStore.LightweightSession();

        documentSession.Listeners.Add(MartenEventsChangesListener.Instance);

        var exception = await Record.ExceptionAsync(async () =>
            await AppendEvents(documentSession, shoppingCartId, events, CancellationToken.None)
        );
        exception.Should().BeNull();
        MartenEventsChangesListener.Instance.AppendedEventsCount.Should().Be(events.Length);
    }
}
