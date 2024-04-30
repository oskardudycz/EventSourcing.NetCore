using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.BusinessLogic.Immutable;
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

public record ProductItem(
    Guid ProductId,
    int Quantity
);

public record PricedProductItem(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
);


public static class ShoppingCartExtensions
{
    public static ShoppingCart GetShoppingCart(this IEnumerable<object> events) =>
        events.Aggregate(ShoppingCart.Default(), ShoppingCart.When);
}

// Business logic

public class BusinessLogicTests
{
    [Fact]
    public void RunningSequenceOfBusinessLogic_ShouldGenerateSequenceOfEvents()
    {
        var shoppingCartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var shoesId = Guid.NewGuid();
        var tShirtId = Guid.NewGuid();
        var twoPairsOfShoes = new ProductItem(shoesId, 2);
        var pairOfShoes = new ProductItem(shoesId, 1);
        var tShirt = new ProductItem(tShirtId, 1);

        var shoesPrice = 100;
        var tShirtPrice = 50;

        var pricedPairOfShoes = new PricedProductItem(shoesId, 1, shoesPrice);
        var pricedTShirt = new PricedProductItem(tShirtId, 1, tShirtPrice);

        var events = new List<object>();

        events.Add(
            OpenShoppingCart.Handle(OpenShoppingCart.From(shoppingCartId, clientId))
        );
        events.Add(
            AddProductItemToShoppingCart.Handle(
                FakeProductPriceCalculator.Returning(shoesPrice),
                AddProductItemToShoppingCart.From(shoppingCartId, twoPairsOfShoes),
                events.GetShoppingCart()
            )
        );
        events.Add(
            AddProductItemToShoppingCart.Handle(
                FakeProductPriceCalculator.Returning(tShirtPrice),
                AddProductItemToShoppingCart.From(shoppingCartId, tShirt),
                events.GetShoppingCart()
            )
        );
        events.Add(
            RemoveProductItemFromShoppingCart.Handle(
                RemoveProductItemFromShoppingCart.From(shoppingCartId,
                    pricedPairOfShoes
                ),
                events.GetShoppingCart()
            )
        );
        events.Add(
            ConfirmShoppingCart.Handle(
                ConfirmShoppingCart.From(shoppingCartId),
                events.GetShoppingCart()
            )
        );

        var exception = Record.Exception(() => events.Add(
            CancelShoppingCart.Handle(
                CancelShoppingCart.From(shoppingCartId),
                events.GetShoppingCart()
            )
        ));
        exception.Should().BeOfType<InvalidOperationException>();

        var shoppingCart = events.GetShoppingCart();

        shoppingCart.Id.Should().Be(shoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.ProductItems.Should().HaveCount(2);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);

        shoppingCart.ProductItems[0].Should().Be(pricedPairOfShoes);
        shoppingCart.ProductItems[1].Should().Be(pricedTShirt);
    }
}
