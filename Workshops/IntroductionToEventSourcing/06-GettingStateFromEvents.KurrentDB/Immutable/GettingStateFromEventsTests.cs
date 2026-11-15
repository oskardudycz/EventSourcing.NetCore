using KurrentDB.Client;
using Shouldly;
using IntroductionToEventSourcing.GettingStateFromEvents.Tools;
using Xunit;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Immutable;
using static ShoppingCartEvent;

// EVENTS
public abstract record ShoppingCartEvent
{
    public record ShoppingCartOpened(
        Guid ShoppingCartId,
        Guid ClientId
    ): ShoppingCartEvent;

    public record ProductItemAddedToShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem
    ): ShoppingCartEvent;

    public record ProductItemRemovedFromShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem
    ): ShoppingCartEvent;

    public record ShoppingCartConfirmed(
        Guid ShoppingCartId,
        DateTime ConfirmedAt
    ): ShoppingCartEvent;

    public record ShoppingCartCanceled(
        Guid ShoppingCartId,
        DateTime CanceledAt
    ): ShoppingCartEvent;

    // This won't allow external inheritance
    private ShoppingCartEvent(){}
}

// VALUE OBJECTS
public record PricedProductItem(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
);

// ENTITY
public record ShoppingCart(
    Guid Id,
    Guid ClientId,
    ShoppingCartStatus Status,
    PricedProductItem[] ProductItems,
    DateTime? ConfirmedAt = null,
    DateTime? CanceledAt = null
);

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Canceled = 4
}

public class GettingStateFromEventsTests: KurrentDBTest
{
    /// <summary>
    /// Solution - Mutable entity with When method
    /// </summary>
    /// <returns></returns>
    private static Task<ShoppingCart> GetShoppingCart(KurrentDBClient eventStore, string streamName, CancellationToken ct) =>
        // 1. Add logic here
        throw new NotImplementedException();

    [Fact]
    [Trait("Category", "SkipCI")]
    public async Task GettingState_FromKurrentDB_ShouldSucceed()
    {
        var shoppingCartId = Guid.CreateVersion7();
        var clientId = Guid.CreateVersion7();
        var shoesId = Guid.CreateVersion7();
        var tShirtId = Guid.CreateVersion7();
        var twoPairsOfShoes = new PricedProductItem(shoesId, 2, 100);
        var pairOfShoes = new PricedProductItem(shoesId, 1, 100);
        var tShirt = new PricedProductItem(tShirtId, 1, 50);

        var events = new ShoppingCartEvent[]
        {
            new ShoppingCartOpened(shoppingCartId, clientId),
            new ProductItemAddedToShoppingCart(shoppingCartId, twoPairsOfShoes),
            new ProductItemAddedToShoppingCart(shoppingCartId, tShirt),
            new ProductItemRemovedFromShoppingCart(shoppingCartId, pairOfShoes),
            new ShoppingCartConfirmed(shoppingCartId, DateTime.UtcNow),
            new ShoppingCartCanceled(shoppingCartId, DateTime.UtcNow)
        };

        var streamName = $"shopping_cart-{shoppingCartId}";

        await AppendEvents(streamName, events, CancellationToken.None);

        var shoppingCart = await GetShoppingCart(EventStore, streamName, CancellationToken.None);

        shoppingCart.Id.ShouldBe(shoppingCartId);
        shoppingCart.ClientId.ShouldBe(clientId);
        shoppingCart.ProductItems.Count().ShouldBe(2);

        shoppingCart.ProductItems[0].ProductId.ShouldBe(shoesId);
        shoppingCart.ProductItems[0].Quantity.ShouldBe(pairOfShoes.Quantity);
        shoppingCart.ProductItems[0].UnitPrice.ShouldBe(pairOfShoes.UnitPrice);

        shoppingCart.ProductItems[1].ProductId.ShouldBe(tShirtId);
        shoppingCart.ProductItems[1].Quantity.ShouldBe(tShirt.Quantity);
        shoppingCart.ProductItems[1].UnitPrice.ShouldBe(tShirt.UnitPrice);
    }
}
