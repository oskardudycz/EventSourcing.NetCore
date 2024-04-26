using FluentAssertions;
using IntroductionToEventSourcing.BusinessLogic.Tools;
using Xunit;

namespace IntroductionToEventSourcing.BusinessLogic.Mutable;

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

    // This won't allow
    private ShoppingCartEvent(){}
}

// VALUE OBJECTS
public class PricedProductItem
{
    public Guid ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public class ProductItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class BusinessLogicTests: MartenTest
{
    [Fact]
    public async Task GettingState_ForSequenceOfEvents_ShouldSucceed()
    {
        var shoppingCartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var shoesId = Guid.NewGuid();
        var tShirtId = Guid.NewGuid();

        var twoPairsOfShoes = new ProductItem { ProductId = shoesId, Quantity = 2 };
        var pairOfShoes = new ProductItem { ProductId = shoesId, Quantity = 1 };
        var tShirt = new ProductItem { ProductId = tShirtId, Quantity = 1 };

        var shoesPrice = 100;
        var tShirtPrice = 50;

        var pricedPairOfShoes = new PricedProductItem { ProductId = shoesId, Quantity = 1, UnitPrice = shoesPrice };
        var pricedTShirt = new PricedProductItem{ ProductId = tShirtId, Quantity = 1, UnitPrice = tShirtPrice };

        // Open
        await DocumentSession.Add(
            command => command.ShoppingCartId,
            command =>
                ShoppingCart.Open(command.ShoppingCartId, command.ClientId),
            OpenShoppingCart.From(shoppingCartId, clientId),
            CancellationToken.None
        );

        // Add two pairs of shoes
        await DocumentSession.GetAndUpdate<ShoppingCart, AddProductItemToShoppingCart>(
            command => command.ShoppingCartId,
            (command, shoppingCart) =>
                shoppingCart.AddProduct(FakeProductPriceCalculator.Returning(shoesPrice), command.ProductItem),
            AddProductItemToShoppingCart.From(shoppingCartId, twoPairsOfShoes),
            CancellationToken.None
        );

        // Add T-Shirt
        await DocumentSession.GetAndUpdate<ShoppingCart, AddProductItemToShoppingCart>(
            command => command.ShoppingCartId,
            (command, shoppingCart) =>
                shoppingCart.AddProduct(FakeProductPriceCalculator.Returning(tShirtPrice), command.ProductItem),
            AddProductItemToShoppingCart.From(shoppingCartId, tShirt),
            CancellationToken.None
        );

        // Remove pair of shoes
        await DocumentSession.GetAndUpdate<ShoppingCart, RemoveProductItemFromShoppingCart>(
            command => command.ShoppingCartId,
            (command, shoppingCart) =>
                shoppingCart.RemoveProduct(command.ProductItem),
            RemoveProductItemFromShoppingCart.From(shoppingCartId, pricedPairOfShoes),
            CancellationToken.None
        );

        // Confirm
        await DocumentSession.GetAndUpdate<ShoppingCart, ConfirmShoppingCart>(
            command => command.ShoppingCartId,
            (_, shoppingCart) =>
                shoppingCart.Confirm(),
            ConfirmShoppingCart.From(shoppingCartId),
            CancellationToken.None
        );

        // Cancel
        var exception = await Record.ExceptionAsync(async () =>
            {
                await DocumentSession.GetAndUpdate<ShoppingCart, CancelShoppingCart>(
                    command => command.ShoppingCartId,
                    (_, shoppingCart) =>
                        shoppingCart.Cancel(),
                    CancelShoppingCart.From(shoppingCartId),
                    CancellationToken.None
                );
            }
        );
        exception.Should().BeOfType<InvalidOperationException>();

        var shoppingCart = await DocumentSession.Get<ShoppingCart>(shoppingCartId, CancellationToken.None);

        shoppingCart.Id.Should().Be(shoppingCartId);
        shoppingCart.ClientId.Should().Be(clientId);
        shoppingCart.ProductItems.Should().HaveCount(2);
        shoppingCart.Status.Should().Be(ShoppingCartStatus.Confirmed);

        shoppingCart.ProductItems[0].ProductId.Should().Be(shoesId);
        shoppingCart.ProductItems[0].Quantity.Should().Be(pricedPairOfShoes.Quantity);
        shoppingCart.ProductItems[0].UnitPrice.Should().Be(pricedPairOfShoes.UnitPrice);

        shoppingCart.ProductItems[1].ProductId.Should().Be(tShirtId);
        shoppingCart.ProductItems[1].Quantity.Should().Be(pricedTShirt.Quantity);
        shoppingCart.ProductItems[1].UnitPrice.Should().Be(pricedTShirt.UnitPrice);
    }
}
