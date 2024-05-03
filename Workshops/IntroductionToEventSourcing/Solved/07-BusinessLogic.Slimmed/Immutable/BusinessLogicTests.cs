using IntroductionToEventSourcing.BusinessLogic.Slimmed.Immutable.Pricing;
using Ogooreck.BusinessLogic;
using Xunit;

namespace IntroductionToEventSourcing.BusinessLogic.Slimmed.Immutable;

using static ShoppingCart.Event;
using static ShoppingCart.Command;
using static ShoppingCart;

public class BusinessLogicTests
{
    // Open
    [Fact]
    public void OpensShoppingCart() =>
        Spec.Given([])
            .When(new Open(clientId, now))
            .Then(new Opened(clientId, now));

    // Add
    [Fact]
    public void CantAddProductItemToNotExistingShoppingCart() =>
        Spec.Given([])
            .When(
                new AddProductItem(FakeProductPriceCalculator.Returning(Price).Calculate(ProductItem), now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void AddsProductItemToEmptyShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now)
            ])
            .When(new AddProductItem(FakeProductPriceCalculator.Returning(Price).Calculate(ProductItem), now))
            .Then(
                new ProductItemAdded(
                    new PricedProductItem(ProductItem.ProductId, ProductItem.Quantity, Price),
                    now
                )
            );


    [Fact]
    public void AddsProductItemToNonEmptyShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
                new ProductItemAdded(pricedProductItem, now),
            ])
            .When(
                new AddProductItem(FakeProductPriceCalculator.Returning(OtherPrice).Calculate(OtherProductItem), now)
            )
            .Then(
                new ProductItemAdded(
                    new PricedProductItem(OtherProductItem.ProductId, OtherProductItem.Quantity, OtherPrice),
                    now
                )
            );

    [Fact]
    public void CantAddProductItemToConfirmedShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
                new ProductItemAdded(pricedProductItem, now),
                new Confirmed(now)
            ])
            .When(
                new AddProductItem(FakeProductPriceCalculator.Returning(Price).Calculate(ProductItem), now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantAddProductItemToCanceledShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
                new ProductItemAdded(pricedProductItem, now),
                new Canceled(now)
            ])
            .When(
                new AddProductItem(FakeProductPriceCalculator.Returning(Price).Calculate(ProductItem), now)
            )
            .ThenThrows<InvalidOperationException>();

    // Remove
    [Fact]
    public void CantRemoveProductItemFromNotExistingShoppingCart() =>
        Spec.Given([])
            .When(
                new RemoveProductItem(pricedProductItem, now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void RemovesExistingProductItemFromShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
                new ProductItemAdded(pricedProductItem, now),
            ])
            .When(
                new RemoveProductItem(pricedProductItem with { Quantity = 1 }, now)
            )
            .Then(
                new ProductItemRemoved(
                    pricedProductItem with { Quantity = 1 },
                    now
                )
            );

    [Fact]
    public void CantRemoveNonExistingProductItemFromNonEmptyShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
                new ProductItemAdded(pricedProductItem, now),
            ])
            .When(
                new RemoveProductItem(otherPricedProductItem with { Quantity = 1 }, now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantRemoveExistingProductItemFromCanceledShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
                new ProductItemAdded(pricedProductItem, now),
                new Confirmed(now)
            ])
            .When(
                new RemoveProductItem(pricedProductItem with { Quantity = 1 }, now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantRemoveExistingProductItemFromConfirmedShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
                new ProductItemAdded(pricedProductItem, now),
                new Canceled(now)
            ])
            .When(
                new RemoveProductItem(pricedProductItem with { Quantity = 1 }, now)
            )
            .ThenThrows<InvalidOperationException>();

    // Confirm

    [Fact]
    public void CantConfirmNotExistingShoppingCart() =>
        Spec.Given([])
            .When(
                new Confirm(now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    [Trait("Category", "SkipCI")]
    public void CantConfirmEmptyShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
            ])
            .When(
                new Confirm(now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void ConfirmsNonEmptyShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
                new ProductItemAdded(pricedProductItem, now),
            ])
            .When(
                new Confirm(now)
            )
            .Then(
                new Confirmed(now)
            );

    [Fact]
    public void CantConfirmAlreadyConfirmedShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
                new ProductItemAdded(pricedProductItem, now),
                new Confirmed(now)
            ])
            .When(
                new Confirm(now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantConfirmCanceledShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
                new ProductItemAdded(pricedProductItem, now),
                new Canceled(now)
            ])
            .When(
                new Confirm(now)
            )
            .ThenThrows<InvalidOperationException>();

    // Cancel
    [Fact]
    [Trait("Category", "SkipCI")]
    public void CantCancelNotExistingShoppingCart() =>
        Spec.Given([])
            .When(
                new Cancel(now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CancelsEmptyShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
            ])
            .When(
                new Cancel(now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CancelsNonEmptyShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
                new ProductItemAdded(pricedProductItem, now),
            ])
            .When(
                new Cancel(now)
            )
            .Then(
                new Canceled(now)
            );

    [Fact]
    public void CantCancelAlreadyCanceledShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
                new ProductItemAdded(pricedProductItem, now),
                new Canceled(now)
            ])
            .When(
                new Cancel(now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantCancelConfirmedShoppingCart() =>
        Spec.Given([
                new Opened(clientId, now),
                new ProductItemAdded(pricedProductItem, now),
                new Confirmed(now)
            ])
            .When(
                new Cancel(now)
            )
            .ThenThrows<InvalidOperationException>();

    private readonly DateTimeOffset now = DateTimeOffset.Now;
    private readonly Guid clientId = Guid.NewGuid();
    private static readonly ProductItem ProductItem = new(Guid.NewGuid(), Random.Shared.Next(1, 200));
    private static readonly ProductItem OtherProductItem = new(Guid.NewGuid(), Random.Shared.Next(1, 200));
    private static readonly int Price = Random.Shared.Next(1, 1000);
    private static readonly int OtherPrice = Random.Shared.Next(1, 1000);

    private readonly PricedProductItem pricedProductItem =
        new(ProductItem.ProductId, ProductItem.Quantity, Price);

    private readonly PricedProductItem otherPricedProductItem =
        new(OtherProductItem.ProductId, OtherProductItem.Quantity, Price);


    private readonly DeciderSpecification<Command, Event, ShoppingCart> Spec =
        Specification.For<Command, Event, ShoppingCart>(Decide, Evolve, () => new Initial());
}
