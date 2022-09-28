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

public interface IVersioned
{
    ulong Version { get; set; }
}

public class ShoppingCartDetails: IVersioned
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public ShoppingCartStatus Status { get; set; }
    public IList<PricedProductItem> ProductItems { get; set; } = new List<PricedProductItem>();
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CanceledAt { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TotalItemsCount { get; set; }

    public ulong Version { get; set; }
}

public class ShoppingCartShortInfo: IVersioned
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TotalItemsCount { get; set; }
    public ulong Version { get; set; }
}

public static class DatabaseExtensions
{
    public static T? GetExpectingGreaterOrEqualVersionWithRetries<T>(this Database database, Guid id, ulong expectedVersion)
        where T : class, IVersioned, new()
    {
        T? item;
        var triesLeft = 4;

        do
        {
            item = database.Get<T>(id);

            if (item != null && item.Version < expectedVersion)
                item = null;

            triesLeft--;

            if(triesLeft > 0) Thread.Sleep(50);

        } while (item == null && triesLeft > 0);

        return item;
    }

    public static void GetAndStore<T>(this Database database, Guid id, ulong currentVersion, Func<T, T> update)
        where T : class, IVersioned, new()
    {
        var expectedVersion = currentVersion - 1;

        var item = database.GetExpectingGreaterOrEqualVersionWithRetries<T>(id, expectedVersion);

        if (item == null)
            throw new Exception($"Item with id: '{id}' and expected version: {expectedVersion} not found!");

        if (item.Version > expectedVersion)
            return;

        item.Version = currentVersion;

        database.Store(id, update(item));
    }
}

public class ShoppingCartDetailsProjection
{
    private readonly Database database;
    public ShoppingCartDetailsProjection(Database database) => this.database = database;

    public void Handle(EventEnvelope<ShoppingCartOpened> @event) =>
        database.Store(
            @event.Data.ShoppingCartId,
            new ShoppingCartDetails
            {
                Id = @event.Data.ShoppingCartId,
                Status = ShoppingCartStatus.Pending,
                ClientId = @event.Data.ClientId,
                ProductItems = new List<PricedProductItem>(),
                TotalPrice = 0,
                TotalItemsCount = 0,
                Version = @event.Metadata.StreamPosition
            });

    public void Handle(EventEnvelope<ProductItemAddedToShoppingCart> @event) =>
        database.GetAndStore<ShoppingCartDetails>(
            @event.Data.ShoppingCartId,
            @event.Metadata.StreamPosition,
            item =>
            {
                var productItem = @event.Data.ProductItem;
                var existingProductItem = item.ProductItems.SingleOrDefault(p => p.ProductId == productItem.ProductId);

                if (existingProductItem == null)
                {
                    item.ProductItems.Add(productItem);
                }
                else
                {
                    item.ProductItems.Remove(existingProductItem);
                    item.ProductItems.Add(
                        new PricedProductItem(
                            existingProductItem.ProductId,
                            existingProductItem.Quantity + productItem.Quantity,
                            existingProductItem.UnitPrice
                        )
                    );
                }

                item.TotalPrice += productItem.TotalAmount;
                item.TotalItemsCount += productItem.Quantity;

                return item;
            });

    public void Handle(EventEnvelope<ProductItemRemovedFromShoppingCart> @event) =>
        database.GetAndStore<ShoppingCartDetails>(
            @event.Data.ShoppingCartId,
            @event.Metadata.StreamPosition,
            item =>
            {
                var productItem = @event.Data.ProductItem;
                var existingProductItem = item.ProductItems.SingleOrDefault(p => p.ProductId == productItem.ProductId);

                if (existingProductItem == null || existingProductItem.Quantity - productItem.Quantity < 0)
                    // You may consider throwing exception here, depending on your strategy
                    return item;

                if (existingProductItem.Quantity - productItem.Quantity == 0)
                {
                    item.ProductItems.Remove(productItem);
                }
                else
                {
                    item.ProductItems.Remove(existingProductItem);
                    item.ProductItems.Add(
                        new PricedProductItem(
                            existingProductItem.ProductId,
                            existingProductItem.Quantity - productItem.Quantity,
                            existingProductItem.UnitPrice
                        )
                    );
                }

                item.TotalPrice -= productItem.TotalAmount;
                item.TotalItemsCount -= productItem.Quantity;

                return item;
            });

    public void Handle(EventEnvelope<ShoppingCartConfirmed> @event) =>
        database.GetAndStore<ShoppingCartDetails>(
            @event.Data.ShoppingCartId,
            @event.Metadata.StreamPosition,
            item =>
            {
                item.Status = ShoppingCartStatus.Confirmed;
                item.ConfirmedAt = DateTime.UtcNow;

                return item;
            });


    public void Handle(EventEnvelope<ShoppingCartCanceled> @event) =>
        database.GetAndStore<ShoppingCartDetails>(
            @event.Data.ShoppingCartId,
            @event.Metadata.StreamPosition,
            item =>
            {
                item.Status = ShoppingCartStatus.Canceled;
                item.CanceledAt = DateTime.UtcNow;

                return item;
            });
}

public class ShoppingCartShortInfoProjection
{
    private readonly Database database;

    public ShoppingCartShortInfoProjection(Database database) => this.database = database;

    public void Handle(EventEnvelope<ShoppingCartOpened> @event) =>
        database.Store(
            @event.Data.ShoppingCartId,
            new ShoppingCartShortInfo
            {
                Id = @event.Data.ShoppingCartId,
                ClientId = @event.Data.ClientId,
                TotalPrice = 0,
                TotalItemsCount = 0,
                Version = @event.Metadata.StreamPosition
            });

    public void Handle(EventEnvelope<ProductItemAddedToShoppingCart> @event) =>
        database.GetAndStore<ShoppingCartShortInfo>(
            @event.Data.ShoppingCartId,
            @event.Metadata.StreamPosition,
            item =>
            {
                var productItem = @event.Data.ProductItem;

                item.TotalPrice += productItem.TotalAmount;
                item.TotalItemsCount += productItem.Quantity;

                return item;
            });

    public void Handle(EventEnvelope<ProductItemRemovedFromShoppingCart> @event) =>
        database.GetAndStore<ShoppingCartShortInfo>(
            @event.Data.ShoppingCartId,
            @event.Metadata.StreamPosition,
            item =>
            {
                var productItem = @event.Data.ProductItem;

                item.TotalPrice -= productItem.TotalAmount;
                item.TotalItemsCount -= productItem.Quantity;

                return item;
            });

    public void Handle(EventEnvelope<ShoppingCartConfirmed> @event) =>
        database.Delete<ShoppingCartShortInfo>(@event.Data.ShoppingCartId);


    public void Handle(EventEnvelope<ShoppingCartCanceled> @event) =>
        database.Delete<ShoppingCartShortInfo>(@event.Data.ShoppingCartId);
}

public class ProjectionsTests
{
    [Fact]
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

        var eventBus = new EventStore();
        var database = new Database();

        // TODO:
        // 1. Register here your event handlers using `eventBus.Register`.
        // 2. Store results in database.
        var shoppingCartDetailsProjection = new ShoppingCartDetailsProjection(database);

        eventBus.Register<ShoppingCartOpened>(shoppingCartDetailsProjection.Handle);
        eventBus.Register<ProductItemAddedToShoppingCart>(shoppingCartDetailsProjection.Handle);
        eventBus.Register<ProductItemRemovedFromShoppingCart>(shoppingCartDetailsProjection.Handle);
        eventBus.Register<ShoppingCartConfirmed>(shoppingCartDetailsProjection.Handle);
        eventBus.Register<ShoppingCartCanceled>(shoppingCartDetailsProjection.Handle);

        var shoppingCartShortInfoProjection = new ShoppingCartShortInfoProjection(database);

        eventBus.Register<ShoppingCartOpened>(shoppingCartShortInfoProjection.Handle);
        eventBus.Register<ProductItemAddedToShoppingCart>(shoppingCartShortInfoProjection.Handle);
        eventBus.Register<ProductItemRemovedFromShoppingCart>(shoppingCartShortInfoProjection.Handle);
        eventBus.Register<ShoppingCartConfirmed>(shoppingCartShortInfoProjection.Handle);
        eventBus.Register<ShoppingCartCanceled>(shoppingCartShortInfoProjection.Handle);

        eventBus.Append(events);

        // first confirmed
        var shoppingCart = database.GetExpectingGreaterOrEqualVersionWithRetries<ShoppingCartDetails>(shoppingCartId, 5)!;
        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(shoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);
        shoppingCart.ProductItems.Should().HaveCount(2);
        shoppingCart.ProductItems.Should().Contain(pairOfShoes);
        shoppingCart.ProductItems.Should().Contain(tShirt);

        var shoppingCartShortInfo = database.GetExpectingGreaterOrEqualVersionWithRetries<ShoppingCartShortInfo>(shoppingCartId, 5);
        shoppingCartShortInfo.Should().BeNull();

        // cancelled
        shoppingCart = database.GetExpectingGreaterOrEqualVersionWithRetries<ShoppingCartDetails>(cancelledShoppingCartId, 3)!;
        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(cancelledShoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Canceled);
        shoppingCart.ProductItems.Should().HaveCount(1);
        shoppingCart.ProductItems.Should().Contain(dress);

        shoppingCartShortInfo = database.GetExpectingGreaterOrEqualVersionWithRetries<ShoppingCartShortInfo>(cancelledShoppingCartId, 3)!;
        shoppingCartShortInfo.Should().BeNull();

        // confirmed but other client
        shoppingCart = database.GetExpectingGreaterOrEqualVersionWithRetries<ShoppingCartDetails>(otherClientShoppingCartId, 3)!;
        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(otherClientShoppingCartId);
        shoppingCart.ClientId.Should().Be(otherClientId);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);
        shoppingCart.ProductItems.Should().HaveCount(1);
        shoppingCart.ProductItems.Should().Contain(dress);

        shoppingCartShortInfo = database.GetExpectingGreaterOrEqualVersionWithRetries<ShoppingCartShortInfo>(otherClientShoppingCartId, 3);
        shoppingCartShortInfo.Should().BeNull();

        // second confirmed
        shoppingCart = database.GetExpectingGreaterOrEqualVersionWithRetries<ShoppingCartDetails>(otherConfirmedShoppingCartId, 3)!;
        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(otherConfirmedShoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);
        shoppingCart.ProductItems.Should().HaveCount(1);
        shoppingCart.ProductItems.Should().Contain(trousers);

        shoppingCartShortInfo = database.GetExpectingGreaterOrEqualVersionWithRetries<ShoppingCartShortInfo>(otherConfirmedShoppingCartId, 3);
        shoppingCartShortInfo.Should().BeNull();

        // first pending
        shoppingCart = database.GetExpectingGreaterOrEqualVersionWithRetries<ShoppingCartDetails>(otherPendingShoppingCartId, 1)!;
        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(otherPendingShoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Pending);
        shoppingCart.ProductItems.Should().BeEmpty();

        shoppingCartShortInfo = database.GetExpectingGreaterOrEqualVersionWithRetries<ShoppingCartShortInfo>(otherPendingShoppingCartId, 1)!;
        shoppingCartShortInfo.Should().NotBeNull();
        shoppingCartShortInfo.Id.Should().Be(otherPendingShoppingCartId);
        shoppingCartShortInfo.ClientId.Should().Be(clientId);
    }
}
