using Ogooreck.BusinessLogic;
using Xunit;

namespace IntroductionToEventSourcing.BusinessLogic.Slimmed.Immutable;

using static ShoppingCartEvent;
using static ShoppingCartCommand;
using static ShoppingCartService;

public class BusinessLogicTests
{
    // Open
    [Fact]
    public void OpensShoppingCart() =>
        Spec.Given([])
            .When(() => Handle(new OpenShoppingCart(shoppingCartId, clientId, now)))
            .Then(new ShoppingCartOpened(shoppingCartId, clientId, now));

    // Add
    [Fact]
    public void CantAddProductItemToNotExistingShoppingCart() =>
        Spec.Given([])
            .When(state =>
                Handle(
                    FakeProductPriceCalculator.Returning(price),
                    new AddProductItemToShoppingCart(shoppingCartId, productItem, now),
                    state
                )
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void AddsProductItemToEmptyShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now)
            ])
            .When(state =>
                Handle(
                    FakeProductPriceCalculator.Returning(price),
                    new AddProductItemToShoppingCart(shoppingCartId, productItem, now),
                    state
                )
            )
            .Then(
                new ProductItemAddedToShoppingCart(
                    shoppingCartId,
                    new PricedProductItem(productItem.ProductId, productItem.Quantity, price),
                    now
                )
            );


    [Fact]
    public void AddsProductItemToNonEmptyShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, pricedProductItem, now),
            ])
            .When(state =>
                Handle(
                    FakeProductPriceCalculator.Returning(otherPrice),
                    new AddProductItemToShoppingCart(shoppingCartId, otherProductItem, now),
                    state
                )
            )
            .Then(
                new ProductItemAddedToShoppingCart(
                    shoppingCartId,
                    new PricedProductItem(otherProductItem.ProductId, otherProductItem.Quantity, otherPrice),
                    now
                )
            );

    [Fact]
    public void CantAddProductItemToConfirmedShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, pricedProductItem, now),
                new ShoppingCartConfirmed(shoppingCartId, now)
            ])
            .When(state =>
                Handle(
                    FakeProductPriceCalculator.Returning(price),
                    new AddProductItemToShoppingCart(shoppingCartId, productItem, now),
                    state
                )
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantAddProductItemToCanceledShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, pricedProductItem, now),
                new ShoppingCartCanceled(shoppingCartId, now)
            ])
            .When(state =>
                Handle(
                    FakeProductPriceCalculator.Returning(price),
                    new AddProductItemToShoppingCart(shoppingCartId, productItem, now),
                    state
                )
            )
            .ThenThrows<InvalidOperationException>();

    // Remove
    [Fact]
    public void CantRemoveProductItemFromNotExistingShoppingCart() =>
        Spec.Given([])
            .When(state =>
                Handle(
                    new RemoveProductItemFromShoppingCart(shoppingCartId, pricedProductItem, now),
                    state
                )
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void RemovesExistingProductItemFromShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, pricedProductItem, now),
            ])
            .When(state =>
                Handle(
                    new RemoveProductItemFromShoppingCart(shoppingCartId, pricedProductItem with { Quantity = 1 }, now),
                    state
                )
            )
            .Then(
                new ProductItemRemovedFromShoppingCart(
                    shoppingCartId,
                    pricedProductItem with { Quantity = 1 },
                    now
                )
            );

    [Fact]
    public void CantRemoveNonExistingProductItemFromEmptyShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, pricedProductItem, now),
            ])
            .When(state =>
                Handle(
                    new RemoveProductItemFromShoppingCart(shoppingCartId, otherPricedProductItem with { Quantity = 1 },
                        now),
                    state
                )
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantRemoveExistingProductItemFromCanceledShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, pricedProductItem, now),
                new ShoppingCartConfirmed(shoppingCartId, now)
            ])
            .When(state =>
                Handle(
                    new RemoveProductItemFromShoppingCart(shoppingCartId, pricedProductItem with { Quantity = 1 }, now),
                    state
                )
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantRemoveExistingProductItemFromConfirmedShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, pricedProductItem, now),
                new ShoppingCartCanceled(shoppingCartId, now)
            ])
            .When(state =>
                Handle(
                    new RemoveProductItemFromShoppingCart(shoppingCartId, pricedProductItem with { Quantity = 1 }, now),
                    state
                )
            )
            .ThenThrows<InvalidOperationException>();

    // Confirm

    [Fact]
    public void CantConfirmNotExistingShoppingCart() =>
        Spec.Given([])
            .When(state =>
                Handle(new ConfirmShoppingCart(shoppingCartId, now), state)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    [Trait("Category", "SkipCI")]
    public void CantConfirmEmptyShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
            ])
            .When(state =>
                Handle(new ConfirmShoppingCart(shoppingCartId, now), state)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void ConfirmsNonEmptyShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, pricedProductItem, now),
            ])
            .When(state =>
                Handle(new ConfirmShoppingCart(shoppingCartId, now), state)
            )
            .Then(
                new ShoppingCartConfirmed(shoppingCartId, now)
            );

    [Fact]
    public void CantConfirmAlreadyConfirmedShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, pricedProductItem, now),
                new ShoppingCartConfirmed(shoppingCartId, now)
            ])
            .When(state =>
                Handle(new ConfirmShoppingCart(shoppingCartId, now), state)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantConfirmCanceledShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, pricedProductItem, now),
                new ShoppingCartCanceled(shoppingCartId, now)
            ])
            .When(state =>
                Handle(new ConfirmShoppingCart(shoppingCartId, now), state)
            )
            .ThenThrows<InvalidOperationException>();

    // Cancel
    [Fact]
    [Trait("Category", "SkipCI")]
    public void CantCancelNotExistingShoppingCart() =>
        Spec.Given([])
            .When(state =>
                Handle(new CancelShoppingCart(shoppingCartId, now), state)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CancelsEmptyShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
            ])
            .When(state =>
                Handle(new CancelShoppingCart(shoppingCartId, now), state)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CancelsNonEmptyShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, pricedProductItem, now),
            ])
            .When(state =>
                Handle(new CancelShoppingCart(shoppingCartId, now), state)
            )
            .Then(
                new ShoppingCartCanceled(shoppingCartId, now)
            );

    [Fact]
    public void CantCancelAlreadyCanceledShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, pricedProductItem, now),
                new ShoppingCartCanceled(shoppingCartId, now)
            ])
            .When(state =>
                Handle(new CancelShoppingCart(shoppingCartId, now), state)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantCancelConfirmedShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, pricedProductItem, now),
                new ShoppingCartConfirmed(shoppingCartId, now)
            ])
            .When(state =>
                Handle(new CancelShoppingCart(shoppingCartId, now), state)
            )
            .ThenThrows<InvalidOperationException>();

    private readonly DateTimeOffset now = DateTimeOffset.Now;
    private readonly Guid clientId = Guid.NewGuid();
    private readonly Guid shoppingCartId = Guid.NewGuid();
    private readonly ProductItem productItem = new(Guid.NewGuid(), Random.Shared.Next(1, 200));
    private readonly ProductItem otherProductItem = new(Guid.NewGuid(), Random.Shared.Next(1, 200));
    private static readonly int price = Random.Shared.Next(1, 1000);
    private static readonly int otherPrice = Random.Shared.Next(1, 1000);
    private readonly PricedProductItem pricedProductItem = new(Guid.NewGuid(), Random.Shared.Next(1, 200), price);

    private readonly PricedProductItem otherPricedProductItem =
        new(Guid.NewGuid(), Random.Shared.Next(1, 200), otherPrice);


    private readonly HandlerSpecification<ShoppingCartEvent, ShoppingCart> Spec =
        Specification.For<ShoppingCartEvent, ShoppingCart>(ShoppingCart.Evolve, ShoppingCart.Default);
}
