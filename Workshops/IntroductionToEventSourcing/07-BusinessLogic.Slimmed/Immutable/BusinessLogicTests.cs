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
                    FakeProductPriceCalculator.Returning(Price),
                    new AddProductItemToShoppingCart(shoppingCartId, ProductItem, now),
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
                    FakeProductPriceCalculator.Returning(Price),
                    new AddProductItemToShoppingCart(shoppingCartId, ProductItem, now),
                    state
                )
            )
            .Then(
                new ProductItemAddedToShoppingCart(
                    shoppingCartId,
                    new PricedProductItem(ProductItem.ProductId, ProductItem.Quantity, Price),
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
                    FakeProductPriceCalculator.Returning(OtherPrice),
                    new AddProductItemToShoppingCart(shoppingCartId, OtherProductItem, now),
                    state
                )
            )
            .Then(
                new ProductItemAddedToShoppingCart(
                    shoppingCartId,
                    new PricedProductItem(OtherProductItem.ProductId, OtherProductItem.Quantity, OtherPrice),
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
                    FakeProductPriceCalculator.Returning(Price),
                    new AddProductItemToShoppingCart(shoppingCartId, ProductItem, now),
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
                    FakeProductPriceCalculator.Returning(Price),
                    new AddProductItemToShoppingCart(shoppingCartId, ProductItem, now),
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
    public void CantRemoveNonExistingProductItemFromNonEmptyShoppingCart() =>
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
    private readonly Guid clientId = Guid.CreateVersion7();
    private readonly Guid shoppingCartId = Guid.CreateVersion7();
    private static readonly ProductItem ProductItem = new(Guid.CreateVersion7(), Random.Shared.Next(1, 200));
    private static readonly ProductItem OtherProductItem = new(Guid.CreateVersion7(), Random.Shared.Next(1, 200));
    private static readonly int Price = Random.Shared.Next(1, 1000);
    private static readonly int OtherPrice = Random.Shared.Next(1, 1000);
    private readonly PricedProductItem pricedProductItem =
        new(ProductItem.ProductId, ProductItem.Quantity, Price);
    private readonly PricedProductItem otherPricedProductItem =
        new(OtherProductItem.ProductId, OtherProductItem.Quantity, Price);


    private readonly HandlerSpecification<ShoppingCartEvent, ShoppingCart> Spec =
        Specification.For<ShoppingCartEvent, ShoppingCart>(ShoppingCart.Evolve, ShoppingCart.Initial);
}
