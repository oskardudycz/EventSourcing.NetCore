using Core.EntityFramework.Projections;
using Core.EntityFramework.Tests.Fixtures;
using Core.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Core.EntityFramework.Tests;

public record Opened(Guid ShoppingCartId, Guid ClientId);

public record ProductItemAdded(Guid ShoppingCartId, int Count);

public record Cancelled(Guid ShoppingCartId);

public class ShoppingCartProjection: EntityFrameworkProjection<ShoppingCart, Guid, TestDbContext>
{
    public ShoppingCartProjection()
    {
        ViewId(c => c.Id);

        Creates<Opened>(Create);
        Projects<ProductItemAdded>(e => e.ShoppingCartId, Apply);
        Deletes<Cancelled>(e => e.ShoppingCartId);
    }

    private ShoppingCart Create(Opened opened) =>
        new() { Id = opened.ShoppingCartId, ClientId = opened.ClientId, ProductCount = 0 };


    private ShoppingCart Apply(ShoppingCart cart, ProductItemAdded added)
    {
        cart.ProductCount += added.Count;
        return cart;
    }
}

public class EntityFrameworkProjectionTests(EfCorePostgresContainerFixture<TestDbContext> fixture)
    : IClassFixture<EfCorePostgresContainerFixture<TestDbContext>>
{
    private readonly TestDbContext context = fixture.DbContext;

    [Fact]
    public async Task Applies_Works_Separately()
    {
        var projection = new ShoppingCartProjection { DbContext = context };

        var cartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        await projection.Handle([EventEnvelope.From(new Opened(cartId, clientId))], CancellationToken.None);
        await context.SaveChangesAsync();

        var savedEntity = await context.ShoppingCarts.Where(e => e.Id == cartId).FirstOrDefaultAsync();
        savedEntity.Should().NotBeNull();
        savedEntity.Should().BeEquivalentTo(new ShoppingCart { Id = cartId, ClientId = clientId, ProductCount = 0 });

        await projection.Handle([EventEnvelope.From(new ProductItemAdded(cartId, 2))], CancellationToken.None);
        await context.SaveChangesAsync();

        savedEntity = await context.ShoppingCarts.Where(e => e.Id == cartId).FirstOrDefaultAsync();
        savedEntity.Should().NotBeNull();
        savedEntity.Should().BeEquivalentTo(new ShoppingCart { Id = cartId, ClientId = clientId, ProductCount = 2 });


        await projection.Handle([EventEnvelope.From(new Cancelled(cartId))], CancellationToken.None);
        await context.SaveChangesAsync();

        savedEntity = await context.ShoppingCarts.Where(e => e.Id == cartId).FirstOrDefaultAsync();
        savedEntity.Should().BeNull();
    }

    [Fact]
    public async Task Applies_Works_In_Batch()
    {
        var projection = new ShoppingCartProjection { DbContext = context };

        var cartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        await projection.Handle([
            EventEnvelope.From(new Opened(cartId, clientId)),
            EventEnvelope.From(new ProductItemAdded(cartId, 2)),
            EventEnvelope.From(new ProductItemAdded(cartId, 3)),
            EventEnvelope.From(new ProductItemAdded(cartId, 5))
        ], CancellationToken.None);
        await context.SaveChangesAsync();

        var savedEntity = await context.ShoppingCarts.Where(e => e.Id == cartId).FirstOrDefaultAsync();
        savedEntity.Should().NotBeNull();
        savedEntity.Should().BeEquivalentTo(new ShoppingCart { Id = cartId, ClientId = clientId, ProductCount = 10 });
    }



    [Fact]
    public async Task Applies_Works_In_Batch_With_AddAndDeleteOnTheSameRecord()
    {
        var projection = new ShoppingCartProjection { DbContext = context };

        var cartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        await projection.Handle([
            EventEnvelope.From(new Opened(cartId, clientId)),
            EventEnvelope.From(new Cancelled(cartId))
        ], CancellationToken.None);
        await context.SaveChangesAsync();

        var savedEntity = await context.ShoppingCarts.Where(e => e.Id == cartId).FirstOrDefaultAsync();
        savedEntity.Should().BeNull();
    }

    [Fact]
    public async Task SmokeTest()
    {
        var entity = new ShoppingCart { Id = Guid.NewGuid(), ProductCount = 2, ClientId = Guid.NewGuid() };
        context.ShoppingCarts.Add(entity);
        await context.SaveChangesAsync();

        var savedEntity = await context.ShoppingCarts.FindAsync(entity.Id);
        Assert.NotNull(savedEntity);
        savedEntity.Should().NotBeNull();
        savedEntity.Should().BeEquivalentTo(entity);
    }
}
