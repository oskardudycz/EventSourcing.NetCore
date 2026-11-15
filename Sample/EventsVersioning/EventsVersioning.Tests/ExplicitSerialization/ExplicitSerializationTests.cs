using System.Text.Json;
using Shouldly;

namespace EventsVersioning.Tests.ExplicitSerialization;

using static ShoppingCartEvent;

public class ExplicitSerializationTests
{
    [Fact]
    public void ShouldSerializeAndDeserializeEvents()
    {
        var shoppingCartId = ShoppingCartId.New();
        var clientId = ClientId.New();

        var tShirt = ProductId.New();
        var tShirtPrice = Price.Parse(new Money(Amount.Parse(33), Currency.PLN));

        var shoes = ProductId.New();
        var shoesPrice = Price.Parse(new Money(Amount.Parse(77), Currency.PLN));

        var events = new ShoppingCartEvent[]
        {
            new ShoppingCartOpened(
                shoppingCartId,
                clientId
            ),
            new ProductItemAddedToShoppingCart(
                shoppingCartId,
                new PricedProductItem(tShirt, Quantity.Parse(5), tShirtPrice)
            ),
            new ProductItemAddedToShoppingCart(
                shoppingCartId,
                new PricedProductItem(shoes, Quantity.Parse(1), shoesPrice)
            ),
            new ProductItemRemovedFromShoppingCart(
                shoppingCartId,
                new PricedProductItem(tShirt, Quantity.Parse(3), tShirtPrice)
            ),
            new ShoppingCartConfirmed(
                shoppingCartId,
                LocalDateTime.Parse(DateTimeOffset.UtcNow)
            )
        };

        var serde = new ShoppingCartEventsSerde();

        var serializedEvents = events.Select(serde.Serialize);

        var deserializedEvents = serializedEvents.Select(e =>
            serde.Deserialize(e.EventType, JsonDocument.Parse(e.Data.ToJsonString()))
        ).ToArray();

        for (var i = 0; i < deserializedEvents.Length; i++)
        {
            deserializedEvents[i].Equals(events[i]).ShouldBeTrue();
        }
    }


    [Fact]
    public void ShouldGetCurrentShoppingCartState()
    {
        var shoppingCartId = ShoppingCartId.New();
        var clientId = ClientId.New();

        var tShirt = ProductId.New();
        var tShirtPrice = Price.Parse(new Money(Amount.Parse(33), Currency.PLN));

        var shoes = ProductId.New();
        var shoesPrice = Price.Parse(new Money(Amount.Parse(77), Currency.PLN));

        var events = new ShoppingCartEvent[]
        {
            new ShoppingCartOpened(
                shoppingCartId,
                clientId
            ),
            new ProductItemAddedToShoppingCart(
                shoppingCartId,
                new PricedProductItem(tShirt, Quantity.Parse(5), tShirtPrice)
            ),
            new ProductItemAddedToShoppingCart(
                shoppingCartId,
                new PricedProductItem(shoes, Quantity.Parse(1), shoesPrice)
            ),
            new ProductItemRemovedFromShoppingCart(
                shoppingCartId,
                new PricedProductItem(tShirt, Quantity.Parse(3), tShirtPrice)
            ),
            new ShoppingCartConfirmed(
                shoppingCartId,
                LocalDateTime.Parse(DateTimeOffset.UtcNow)
            )
        };

        var shoppingCart = events.Aggregate(ShoppingCart.Default, ShoppingCart.Evolve);

        shoppingCart.Id.ShouldBe(shoppingCartId);
        shoppingCart.ClientId.ShouldBe(clientId);
        shoppingCart.Status.ShouldBe(ShoppingCartStatus.Confirmed);
        shoppingCart.ProductItems.Count().ShouldBe(2);
        shoppingCart.ProductItems.Keys.ShouldBe([tShirt, shoes], ignoreOrder: true);
        shoppingCart.ProductItems[tShirt].ShouldBe(Quantity.Parse(2));
        shoppingCart.ProductItems[shoes].ShouldBe(Quantity.Parse(1));
    }
}
