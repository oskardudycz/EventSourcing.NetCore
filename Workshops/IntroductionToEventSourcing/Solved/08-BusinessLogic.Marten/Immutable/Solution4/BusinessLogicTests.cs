using FluentAssertions;
using IntroductionToEventSourcing.BusinessLogic.Tools;
using Xunit;

namespace IntroductionToEventSourcing.BusinessLogic.Immutable.Solution4;

using static ShoppingCart;
using static ShoppingCartCommand;

public class BusinessLogicTests: MartenTest
{
    [Fact]
    public async Task GettingState_ForSequenceOfEvents_ShouldSucceed()
    {
        var shoppingCartId = ShoppingCartId.From(Guid.NewGuid());
        var clientId = ClientId.From(Guid.NewGuid());
        var shoesId = ProductId.From(Guid.NewGuid());
        var tShirtId = ProductId.From(Guid.NewGuid());

        var one = ProductQuantity.From(1);
        var two = ProductQuantity.From(2);

        var twoPairsOfShoes = new ProductItem(shoesId, two);
        var pairOfShoes = new ProductItem(shoesId, one);
        var tShirt = new ProductItem(tShirtId, one);

        var shoesPrice = ProductPrice.From(100);
        var tShirtPrice = ProductPrice.From(50);

        var pricedPairOfShoes = new PricedProductItem(shoesId, one, shoesPrice);
        var pricedTwoPairsOfShoes = new PricedProductItem(shoesId, two, shoesPrice);
        var pricedTShirt = new PricedProductItem(tShirtId, one, tShirtPrice);

        await DocumentSession.Decide(
            shoppingCartId,
            new Open(shoppingCartId, clientId, DateTimeOffset.Now),
            CancellationToken.None
        );

        // Add two pairs of shoes
        await DocumentSession.Decide(
            shoppingCartId,
            new AddProductItem(shoppingCartId, pricedTwoPairsOfShoes),
            CancellationToken.None
        );

        // Add T-Shirt
        await DocumentSession.Decide(
            shoppingCartId,
            new AddProductItem(shoppingCartId, pricedTShirt),
            CancellationToken.None
        );

        // Remove pair of shoes
        await DocumentSession.Decide(
            shoppingCartId,
            new RemoveProductItem(shoppingCartId, pricedPairOfShoes),
            CancellationToken.None
        );


        var pendingShoppingCart =
            await DocumentSession.Get<ShoppingCart>(shoppingCartId.Value, CancellationToken.None) as Pending;

        pendingShoppingCart.Should().NotBeNull();
        pendingShoppingCart!.ProductItems.Should().HaveCount(3);

        pendingShoppingCart.ProductItems[0].Should()
            .Be((pricedTwoPairsOfShoes.ProductId, pricedTwoPairsOfShoes.Quantity.Value));
        pendingShoppingCart.ProductItems[1].Should().Be((pricedTShirt.ProductId, pricedTShirt.Quantity.Value));
        pendingShoppingCart.ProductItems[2].Should().Be((pairOfShoes.ProductId, -pairOfShoes.Quantity.Value));

        // Confirm
        await DocumentSession.Decide(
            shoppingCartId,
            new Confirm(shoppingCartId, DateTimeOffset.Now),
            CancellationToken.None
        );

        // Cancel
        var exception = await Record.ExceptionAsync(() =>
            DocumentSession.Decide(
                shoppingCartId,
                new Cancel(shoppingCartId, DateTimeOffset.Now),
                CancellationToken.None
            )
        );
        exception.Should().BeOfType<InvalidOperationException>();

        var shoppingCart = await DocumentSession.Get<ShoppingCart>(shoppingCartId.Value, CancellationToken.None);

        shoppingCart.Should().BeOfType<Closed>();
    }
}
