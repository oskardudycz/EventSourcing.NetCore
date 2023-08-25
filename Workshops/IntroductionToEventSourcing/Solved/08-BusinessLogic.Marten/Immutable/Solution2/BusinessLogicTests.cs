using FluentAssertions;
using IntroductionToEventSourcing.BusinessLogic.Tools;
using Marten;
using Xunit;

namespace IntroductionToEventSourcing.BusinessLogic.Immutable.Solution2;

using static ShoppingCartCommand;

// Business logic

public class BusinessLogicTests: MartenTest
{
    [Fact]
    public async Task GettingState_ForSequenceOfEvents_ShouldSucceed()
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
        var pricedTwoPairsOfShoes = new PricedProductItem(shoesId, 2, shoesPrice);
        var pricedTShirt = new PricedProductItem(tShirtId, 1, tShirtPrice);

        await DocumentSession.Decide(
            shoppingCartId,
            OpenShoppingCart.From(shoppingCartId, clientId),
            CancellationToken.None
        );

        // Add two pairs of shoes
        await DocumentSession.Decide(
            shoppingCartId,
            AddProductItemToShoppingCart.From(shoppingCartId, pricedTwoPairsOfShoes),
            CancellationToken.None
        );

        // Add T-Shirt
        await DocumentSession.Decide(
            shoppingCartId,
            AddProductItemToShoppingCart.From(shoppingCartId, pricedTShirt),
            CancellationToken.None
        );

        // Remove pair of shoes
        await DocumentSession.Decide(
            shoppingCartId,
            RemoveProductItemFromShoppingCart.From(shoppingCartId, pricedPairOfShoes),
            CancellationToken.None
        );

        // Confirm
        await DocumentSession.Decide(
            shoppingCartId,
            ConfirmShoppingCart.From(shoppingCartId),
            CancellationToken.None
        );

        // Cancel
        var exception = await Record.ExceptionAsync(async () =>
            {
                await DocumentSession.Decide(
                    shoppingCartId,
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

        shoppingCart.ProductItems[0].Should().Be(pricedPairOfShoes);
        shoppingCart.ProductItems[1].Should().Be(pricedTShirt);
    }
}
