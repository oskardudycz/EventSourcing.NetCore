using FluentAssertions;
using IntroductionToEventSourcing.GettingStateFromEvents.Tools;
using Xunit;

namespace IntroductionToEventSourcing.GettingStateFromEvents;

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
)
{
    public decimal TotalAmount => Quantity * UnitPrice;
}

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Canceled = 4
}

public class ShoppingCartDetails
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public ShoppingCartStatus Status { get; set; }
    public IList<PricedProductItem> ProductItems { get; set; } = new List<PricedProductItem>();
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CanceledAt { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TotalItemsCount { get; set; }
}

public class ShoppingCartsClientSummary
{
    public Guid Id { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalItemsCount { get; set; }
}

public class ProjectionsTests
{
    [Fact]
    [Trait("Category", "SkipCI")]
    public void GettingState_ForSequenceOfEvents_ShouldSucceed()
    {
        var shoppingCartId = Guid.NewGuid();

        var clientId = Guid.NewGuid();
        var shoesId = Guid.NewGuid();
        var tShirtId = Guid.NewGuid();
        var dressId = Guid.NewGuid();
        var trousersId = Guid.NewGuid();

        var twoPairsOfShoes = new PricedProductItem(shoesId, 2, 100);
        var pairOfShoes = new PricedProductItem(shoesId, 1, 100);
        var tShirt = new PricedProductItem(tShirtId, 1, 50);
        var dress = new PricedProductItem(dressId, 3, 150);
        var trousers = new PricedProductItem(trousersId, 1, 300);

        var cancelledShoppingCartId = Guid.NewGuid();
        var otherClientShoppingCartId = Guid.NewGuid();
        var otherConfirmedShoppingCartId = Guid.NewGuid();
        var otherPendingShoppingCartId = Guid.NewGuid();
        var otherClientId = Guid.NewGuid();

        var logPosition = 0ul;

        var events = new EventEnvelope[]
        {
            // first confirmed
            new EventEnvelope<ShoppingCartOpened>(new ShoppingCartOpened(shoppingCartId, clientId),
                EventMetadata.For(1, ++logPosition)),
            new EventEnvelope<ProductItemAddedToShoppingCart>(
                new ProductItemAddedToShoppingCart(shoppingCartId, twoPairsOfShoes),
                EventMetadata.For(2, ++logPosition)),
            new EventEnvelope<ProductItemAddedToShoppingCart>(
                new ProductItemAddedToShoppingCart(shoppingCartId, tShirt), EventMetadata.For(3, ++logPosition)),
            new EventEnvelope<ProductItemRemovedFromShoppingCart>(
                new ProductItemRemovedFromShoppingCart(shoppingCartId, pairOfShoes),
                EventMetadata.For(4, ++logPosition)),
            new EventEnvelope<ShoppingCartConfirmed>(new ShoppingCartConfirmed(shoppingCartId, DateTime.UtcNow),
                EventMetadata.For(5, ++logPosition)),

            // cancelled
            new EventEnvelope<ShoppingCartOpened>(new ShoppingCartOpened(cancelledShoppingCartId, clientId),
                EventMetadata.For(1, ++logPosition)),
            new EventEnvelope<ProductItemAddedToShoppingCart>(
                new ProductItemAddedToShoppingCart(cancelledShoppingCartId, dress),
                EventMetadata.For(2, ++logPosition)),
            new EventEnvelope<ShoppingCartCanceled>(new ShoppingCartCanceled(cancelledShoppingCartId, DateTime.UtcNow),
                EventMetadata.For(3, ++logPosition)),

            // confirmed but other client
            new EventEnvelope<ShoppingCartOpened>(new ShoppingCartOpened(otherClientShoppingCartId, otherClientId),
                EventMetadata.For(1, ++logPosition)),
            new EventEnvelope<ProductItemAddedToShoppingCart>(
                new ProductItemAddedToShoppingCart(otherClientShoppingCartId, dress),
                EventMetadata.For(2, ++logPosition)),
            new EventEnvelope<ShoppingCartConfirmed>(
                new ShoppingCartConfirmed(otherClientShoppingCartId, DateTime.UtcNow),
                EventMetadata.For(3, ++logPosition)),

            // second confirmed
            new EventEnvelope<ShoppingCartOpened>(new ShoppingCartOpened(otherConfirmedShoppingCartId, clientId),
                EventMetadata.For(1, ++logPosition)),
            new EventEnvelope<ProductItemAddedToShoppingCart>(
                new ProductItemAddedToShoppingCart(otherConfirmedShoppingCartId, trousers),
                EventMetadata.For(2, ++logPosition)),
            new EventEnvelope<ShoppingCartConfirmed>(
                new ShoppingCartConfirmed(otherConfirmedShoppingCartId, DateTime.UtcNow),
                EventMetadata.For(3, ++logPosition)),

            // first pending
            new EventEnvelope<ShoppingCartOpened>(new ShoppingCartOpened(otherPendingShoppingCartId, clientId),
                EventMetadata.For(1, ++logPosition))
        };

        var eventBus = new EventBus();
        var database = new Database();

        // TODO:
        // 1. Register here your event handlers using `eventBus.Register`.
        // 2. Store results in database.

        eventBus.Publish(events);

        // first confirmed
        var shoppingCart = database.Get<ShoppingCartDetails>(shoppingCartId)!;

        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(shoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.ProductItems.Should().HaveCount(2);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);

        shoppingCart.ProductItems[0].Should().Be(pairOfShoes);
        shoppingCart.ProductItems[1].Should().Be(tShirt);

        // cancelled
        shoppingCart = database.Get<ShoppingCartDetails>(cancelledShoppingCartId)!;

        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(cancelledShoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.ProductItems.Should().HaveCount(1);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Canceled);

        shoppingCart.ProductItems[0].Should().Be(dress);

        // confirmed but other client
        shoppingCart = database.Get<ShoppingCartDetails>(otherClientShoppingCartId)!;

        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(cancelledShoppingCartId);
        shoppingCart.ClientId.Should().Be(otherClientId);
        shoppingCart.ProductItems.Should().HaveCount(1);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);

        shoppingCart.ProductItems[0].Should().Be(dress);

        // second confirmed
        shoppingCart = database.Get<ShoppingCartDetails>(otherConfirmedShoppingCartId)!;

        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(otherConfirmedShoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.ProductItems.Should().HaveCount(1);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);

        shoppingCart.ProductItems[0].Should().Be(trousers);

        // first pending
        shoppingCart = database.Get<ShoppingCartDetails>(otherConfirmedShoppingCartId)!;

        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(otherConfirmedShoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.ProductItems.Should().BeEmpty();
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Pending);

        // summary
        var summary = database.Get<ShoppingCartsClientSummary>(cancelledShoppingCartId)!;
        summary.Id.Should().Be(clientId);
        summary.TotalItemsCount.Should().Be(3);
        summary.TotalAmount.Should().Be(pairOfShoes.TotalAmount + tShirt.TotalAmount + trousers.TotalAmount);
    }
}
