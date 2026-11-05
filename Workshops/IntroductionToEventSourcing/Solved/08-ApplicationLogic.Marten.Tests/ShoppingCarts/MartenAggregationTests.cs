using ApplicationLogic.Marten.Mutable.Pricing;
using ApplicationLogic.Marten.Mutable.ShoppingCarts;
using FluentAssertions;
using global::Marten;
using Testcontainers.PostgreSql;
using Xunit;

namespace ApplicationLogic.Marten.Tests.ShoppingCarts;

public class MartenAggregationTests
{
    [Fact]
    public async Task SingleApplyRestoresCartAndCollectsPendingEventsWithGenericBase()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:15.1-alpine").Build();
        await postgres.StartAsync();

        using var store = DocumentStore.For(options =>
        {
            options.Connection(postgres.GetConnectionString());
            options.Projections.LiveStreamAggregation<MutableShoppingCart>();
        });
        using var session = store.LightweightSession();
        var id = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var cart = MutableShoppingCart.Open(id, clientId, now);
        session.Events.StartStream<MutableShoppingCart>(id, cart.DequeueUncommittedEvents());
        await session.SaveChangesAsync();

        var restored = await session.Events.AggregateStreamAsync<MutableShoppingCart>(id);
        restored!.AddProduct(FakeProductPriceCalculator.Returning(10),
            new ProductItem { ProductId = productId, Quantity = 2 }, now);
        restored.Confirm(now);
        var pendingEvents = restored.DequeueUncommittedEvents();
        pendingEvents.Should().HaveCount(2);
        restored.DequeueUncommittedEvents().Should().BeEmpty();
        session.Events.Append(id, pendingEvents);
        await session.SaveChangesAsync();

        var confirmed = await session.Events.AggregateStreamAsync<MutableShoppingCart>(id);
        confirmed!.Id.Should().Be(id);
        confirmed.ClientId.Should().Be(clientId);
        confirmed.Status.Should().Be(ShoppingCartStatus.Confirmed);
        confirmed.ProductItems.Should().ContainSingle().Which.ProductId.Should().Be(productId);
        confirmed.ProductItems.Single().Quantity.Should().Be(2);
        confirmed.DequeueUncommittedEvents().Should().BeEmpty();
    }
}
