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

public class ShoppingCartShortInfo
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TotalItemsCount { get; set; }
}

public static class DatabaseExtensions
{
    public static void GetAndStore<T>(this Database database, Guid id, Func<T, T> update) where T : class, new ()
    {
        var item = database.Get<T>(id) ?? new T();

        database.Store(id, update(item));
    }
}

public class ShoppingCartDetailsProjection(Database database)
{
    public void Handle(EventEnvelope<ShoppingCartOpened> @event) =>
        database.Store(@event.Data.ShoppingCartId,
            new ShoppingCartDetails
            {
                Id = @event.Data.ShoppingCartId,
                Status = ShoppingCartStatus.Pending,
                ClientId = @event.Data.ClientId,
                ProductItems = new List<PricedProductItem>(),
                TotalPrice = 0,
                TotalItemsCount = 0
            });

    public void Handle(EventEnvelope<ProductItemAddedToShoppingCart> @event) =>
        database.GetAndStore<ShoppingCartDetails>(@event.Data.ShoppingCartId, item =>
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
        database.GetAndStore<ShoppingCartDetails>(@event.Data.ShoppingCartId, item =>
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
        database.GetAndStore<ShoppingCartDetails>(@event.Data.ShoppingCartId, item =>
        {
            item.Status = ShoppingCartStatus.Confirmed;
            item.ConfirmedAt = DateTime.UtcNow;

            return item;
        });


    public void Handle(EventEnvelope<ShoppingCartCanceled> @event) =>
        database.GetAndStore<ShoppingCartDetails>(@event.Data.ShoppingCartId, item =>
        {
            item.Status = ShoppingCartStatus.Canceled;
            item.CanceledAt = DateTime.UtcNow;

            return item;
        });
}

public class ShoppingCartShortInfoProjection(Database database)
{
    public void Handle(EventEnvelope<ShoppingCartOpened> @event) =>
        database.Store(@event.Data.ShoppingCartId,
            new ShoppingCartShortInfo
            {
                Id = @event.Data.ShoppingCartId,
                ClientId = @event.Data.ClientId,
                TotalPrice = 0,
                TotalItemsCount = 0
            });

    public void Handle(EventEnvelope<ProductItemAddedToShoppingCart> @event) =>
        database.GetAndStore<ShoppingCartShortInfo>(@event.Data.ShoppingCartId, item =>
        {
            var productItem = @event.Data.ProductItem;

            item.TotalPrice += productItem.TotalAmount;
            item.TotalItemsCount += productItem.Quantity;

            return item;
        });

    public void Handle(EventEnvelope<ProductItemRemovedFromShoppingCart> @event) =>
        database.GetAndStore<ShoppingCartShortInfo>(@event.Data.ShoppingCartId, item =>
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

        var eventStore = new EventStore();
        var database = new Database();

        // TODO:
        // 1. Register here your event handlers using `eventBus.Register`.
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
        var shoppingCart = database.Get<ShoppingCartDetails>(shoppingCartId)!;
        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(shoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);
        shoppingCart.ProductItems.Should().HaveCount(2);
        shoppingCart.ProductItems.Should().Contain(pairOfShoes);
        shoppingCart.ProductItems.Should().Contain(tShirt);

        var shoppingCartShortInfo = database.Get<ShoppingCartShortInfo>(shoppingCartId);
        shoppingCartShortInfo.Should().BeNull();

        // cancelled
        shoppingCart = database.Get<ShoppingCartDetails>(cancelledShoppingCartId)!;
        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(cancelledShoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Canceled);
        shoppingCart.ProductItems.Should().HaveCount(1);
        shoppingCart.ProductItems.Should().Contain(dress);

        shoppingCartShortInfo = database.Get<ShoppingCartShortInfo>(cancelledShoppingCartId)!;
        shoppingCartShortInfo.Should().BeNull();

        // confirmed but other client
        shoppingCart = database.Get<ShoppingCartDetails>(otherClientShoppingCartId)!;
        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(otherClientShoppingCartId);
        shoppingCart.ClientId.Should().Be(otherClientId);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);
        shoppingCart.ProductItems.Should().HaveCount(1);
        shoppingCart.ProductItems.Should().Contain(dress);

        shoppingCartShortInfo = database.Get<ShoppingCartShortInfo>(otherClientShoppingCartId);
        shoppingCartShortInfo.Should().BeNull();

        // second confirmed
        shoppingCart = database.Get<ShoppingCartDetails>(otherConfirmedShoppingCartId)!;
        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(otherConfirmedShoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);
        shoppingCart.ProductItems.Should().HaveCount(1);
        shoppingCart.ProductItems.Should().Contain(trousers);

        shoppingCartShortInfo = database.Get<ShoppingCartShortInfo>(otherConfirmedShoppingCartId);
        shoppingCartShortInfo.Should().BeNull();

        // first pending
        shoppingCart = database.Get<ShoppingCartDetails>(otherPendingShoppingCartId)!;
        shoppingCart.Should().NotBeNull();
        shoppingCart.Id.Should().Be(otherPendingShoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Pending);
        shoppingCart.ProductItems.Should().BeEmpty();

        shoppingCartShortInfo = database.Get<ShoppingCartShortInfo>(otherPendingShoppingCartId)!;
        shoppingCartShortInfo.Should().NotBeNull();
        shoppingCartShortInfo.Id.Should().Be(otherPendingShoppingCartId);
        shoppingCartShortInfo.ClientId.Should().Be(clientId);
    }
}
