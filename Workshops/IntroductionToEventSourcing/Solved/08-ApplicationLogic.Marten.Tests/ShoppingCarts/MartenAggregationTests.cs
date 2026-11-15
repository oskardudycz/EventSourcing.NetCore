using ApplicationLogic.Marten.Mutable.Pricing;
using ApplicationLogic.Marten.Mutable.ShoppingCarts;
using Shouldly;
using global::Marten;
using Xunit;

namespace ApplicationLogic.Marten.Tests.ShoppingCarts;

public class MartenAggregationTests(TestPostgres postgres)
{
    [Fact]
    public async Task SingleApplyRestoresCartAndCollectsPendingEventsWithGenericBase()
    {
        using var store = DocumentStore.For(options =>
        {
            options.Connection(postgres.ConnectionString);
            options.Projections.LiveStreamAggregation<MutableShoppingCart>();
        });
        using var session = store.LightweightSession();
        var id = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var cart = MutableShoppingCart.Open(id, clientId, now);
        session.Events.StartStream<MutableShoppingCart>(id, cart.DequeueUncommittedEvents());
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        var restored = await session.Events.AggregateStreamAsync<MutableShoppingCart>(id, token: TestContext.Current.CancellationToken);
        restored!.AddProduct(FakeProductPriceCalculator.Returning(10),
            new ProductItem { ProductId = productId, Quantity = 2 }, now);
        restored.Confirm(now);
        var pendingEvents = restored.DequeueUncommittedEvents();
        pendingEvents.Count().ShouldBe(2);
        restored.DequeueUncommittedEvents().ShouldBeEmpty();
        session.Events.Append(id, pendingEvents);
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        var confirmed = await session.Events.AggregateStreamAsync<MutableShoppingCart>(id, token: TestContext.Current.CancellationToken);
        confirmed!.Id.ShouldBe(id);
        confirmed.ClientId.ShouldBe(clientId);
        confirmed.Status.ShouldBe(ShoppingCartStatus.Confirmed);
        confirmed.ProductItems.ShouldHaveSingleItem().ProductId.ShouldBe(productId);
        confirmed.ProductItems.Single().Quantity.ShouldBe(2);
        confirmed.DequeueUncommittedEvents().ShouldBeEmpty();
    }
}
