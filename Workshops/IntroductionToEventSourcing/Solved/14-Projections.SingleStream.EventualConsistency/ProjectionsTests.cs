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

public class ShoppingCartDetailsProjection(Database database)
{
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

public class ShoppingCartShortInfoProjection(Database database)
{
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
        var shoppingCartDetailsProjection = new ShoppingCartDetailsProjection(database);

        eventStore.Register<ShoppingCartOpened>(shoppingCartDetailsProjection.Handle);
        eventStore.Register<ProductItemAddedToShoppingCart>(shoppingCartDetailsProjection.Handle);
        eventStore.Register<ProductItemRemovedFromShoppingCart>(shoppingCartDetailsProjection.Handle);
        eventStore.Register<ShoppingCartConfirmed>(shoppingCartDetailsProjection.Handle);
        eventStore.Register<ShoppingCartCanceled>(shoppingCartDetailsProjection.Handle);

        var shoppingCartShortInfoProjection = new ShoppingCartShortInfoProjection(database);

        eventStore.Register<ShoppingCartOpened>(shoppingCartShortInfoProjection.Handle);
        eventStore.Register<ProductItemAddedToShoppingCart>(shoppingCartShortInfoProjection.Handle);
        eventStore.Register<ProductItemRemovedFromShoppingCart>(shoppingCartShortInfoProjection.Handle);
        eventStore.Register<ShoppingCartConfirmed>(shoppingCartShortInfoProjection.Handle);
        eventStore.Register<ShoppingCartCanceled>(shoppingCartShortInfoProjection.Handle);

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
