using Shouldly;
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

public class ShoppingCartShortInfo
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TotalItemsCount { get; set; }
}

public class ProjectionsTests
{
    [Fact]
    [Trait("Category", "SkipCI")]
    public void GettingState_ForSequenceOfEvents_ShouldSucceed()
    {
        var shoppingCartId = Guid.CreateVersion7();

        var clientId = Guid.CreateVersion7();
        var shoesId = Guid.CreateVersion7();
        var tShirtId = Guid.CreateVersion7();
        var dressId = Guid.CreateVersion7();
        var trousersId = Guid.CreateVersion7();

        var twoPairsOfShoes = new PricedProductItem(shoesId, 2, 100);
        var pairOfShoes = new PricedProductItem(shoesId, 1, 100);
        var tShirt = new PricedProductItem(tShirtId, 1, 50);
        var dress = new PricedProductItem(dressId, 3, 150);
        var trousers = new PricedProductItem(trousersId, 1, 300);

        var cancelledShoppingCartId = Guid.CreateVersion7();
        var otherClientShoppingCartId = Guid.CreateVersion7();
        var otherConfirmedShoppingCartId = Guid.CreateVersion7();
        var otherPendingShoppingCartId = Guid.CreateVersion7();
        var otherClientId = Guid.CreateVersion7();

        var eventStore = new EventStore();
        var database = new Database();

        // TODO:
        // 1. Register here your event handlers using `eventStore.Register`.
        // 2. Store results in database.

        // first confirmed
        eventStore.Append(shoppingCartId, new ShoppingCartOpened(shoppingCartId, clientId));
        eventStore.Append(shoppingCartId, new ProductItemAddedToShoppingCart(shoppingCartId, twoPairsOfShoes));
        eventStore.Append(shoppingCartId, new ProductItemAddedToShoppingCart(shoppingCartId, tShirt));
        eventStore.Append(shoppingCartId, new ProductItemRemovedFromShoppingCart(shoppingCartId, pairOfShoes));
        eventStore.Append(shoppingCartId, new ShoppingCartConfirmed(shoppingCartId, DateTime.UtcNow));

        // cancelled
        eventStore.Append(cancelledShoppingCartId, new ShoppingCartOpened(cancelledShoppingCartId, clientId));
        eventStore.Append(cancelledShoppingCartId, new ProductItemAddedToShoppingCart(cancelledShoppingCartId, dress));
        eventStore.Append(cancelledShoppingCartId, new ShoppingCartCanceled(cancelledShoppingCartId, DateTime.UtcNow));

        // confirmed but other client
        eventStore.Append(otherClientShoppingCartId, new ShoppingCartOpened(otherClientShoppingCartId, otherClientId));
        eventStore.Append(otherClientShoppingCartId, new ProductItemAddedToShoppingCart(otherClientShoppingCartId, dress));
        eventStore.Append(otherClientShoppingCartId, new ShoppingCartConfirmed(otherClientShoppingCartId, DateTime.UtcNow));

        // second confirmed
        eventStore.Append(otherConfirmedShoppingCartId, new ShoppingCartOpened(otherConfirmedShoppingCartId, clientId));
        eventStore.Append(otherConfirmedShoppingCartId, new ProductItemAddedToShoppingCart(otherConfirmedShoppingCartId, trousers));
        eventStore.Append(otherConfirmedShoppingCartId, new ShoppingCartConfirmed(otherConfirmedShoppingCartId, DateTime.UtcNow));

        // first pending
        eventStore.Append(otherPendingShoppingCartId, new ShoppingCartOpened(otherPendingShoppingCartId, clientId));

        // first confirmed
        var shoppingCart = database.Get<ShoppingCartDetails>(shoppingCartId)!;
        shoppingCart.ShouldNotBeNull();
        shoppingCart.Id.ShouldBe(shoppingCartId);
        shoppingCart.ClientId.ShouldBe(clientId);
        shoppingCart.Status.ShouldBe(ShoppingCartStatus.Confirmed);
        shoppingCart.ProductItems.Count().ShouldBe(2);
        shoppingCart.ProductItems.ShouldContain(pairOfShoes);
        shoppingCart.ProductItems.ShouldContain(tShirt);

        var shoppingCartShortInfo = database.Get<ShoppingCartShortInfo>(shoppingCartId);
        shoppingCartShortInfo.ShouldBeNull();

        // cancelled
        shoppingCart = database.Get<ShoppingCartDetails>(cancelledShoppingCartId)!;
        shoppingCart.ShouldNotBeNull();
        shoppingCart.Id.ShouldBe(cancelledShoppingCartId);
        shoppingCart.ClientId.ShouldBe(clientId);
        shoppingCart.Status.ShouldBe(ShoppingCartStatus.Canceled);
        shoppingCart.ProductItems.Count().ShouldBe(1);
        shoppingCart.ProductItems.ShouldContain(dress);

        shoppingCartShortInfo = database.Get<ShoppingCartShortInfo>(cancelledShoppingCartId)!;
        shoppingCartShortInfo.ShouldBeNull();

        // confirmed but other client
        shoppingCart = database.Get<ShoppingCartDetails>(otherClientShoppingCartId)!;
        shoppingCart.ShouldNotBeNull();
        shoppingCart.Id.ShouldBe(otherClientShoppingCartId);
        shoppingCart.ClientId.ShouldBe(otherClientId);
        shoppingCart.Status.ShouldBe(ShoppingCartStatus.Confirmed);
        shoppingCart.ProductItems.Count().ShouldBe(1);
        shoppingCart.ProductItems.ShouldContain(dress);

        shoppingCartShortInfo = database.Get<ShoppingCartShortInfo>(otherClientShoppingCartId);
        shoppingCartShortInfo.ShouldBeNull();

        // second confirmed
        shoppingCart = database.Get<ShoppingCartDetails>(otherConfirmedShoppingCartId)!;
        shoppingCart.ShouldNotBeNull();
        shoppingCart.Id.ShouldBe(otherConfirmedShoppingCartId);
        shoppingCart.ClientId.ShouldBe(clientId);
        shoppingCart.Status.ShouldBe(ShoppingCartStatus.Confirmed);
        shoppingCart.ProductItems.Count().ShouldBe(1);
        shoppingCart.ProductItems.ShouldContain(trousers);

        shoppingCartShortInfo = database.Get<ShoppingCartShortInfo>(otherConfirmedShoppingCartId);
        shoppingCartShortInfo.ShouldBeNull();

        // first pending
        shoppingCart = database.Get<ShoppingCartDetails>(otherPendingShoppingCartId)!;
        shoppingCart.ShouldNotBeNull();
        shoppingCart.Id.ShouldBe(otherPendingShoppingCartId);
        shoppingCart.ClientId.ShouldBe(clientId);
        shoppingCart.Status.ShouldBe(ShoppingCartStatus.Pending);
        shoppingCart.ProductItems.ShouldBeEmpty();

        shoppingCartShortInfo = database.Get<ShoppingCartShortInfo>(otherPendingShoppingCartId)!;
        shoppingCartShortInfo.ShouldNotBeNull();
        shoppingCartShortInfo.Id.ShouldBe(otherPendingShoppingCartId);
        shoppingCartShortInfo.ClientId.ShouldBe(clientId);
    }
}
