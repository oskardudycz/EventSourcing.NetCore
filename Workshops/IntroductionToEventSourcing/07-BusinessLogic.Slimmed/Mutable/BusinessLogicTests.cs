using Ogooreck.BusinessLogic;
using Xunit;

namespace IntroductionToEventSourcing.BusinessLogic.Slimmed.Mutable;

using static ShoppingCartEvent;

public class BusinessLogicTests
{
    // Open
    [Fact]
    public void OpensShoppingCart() =>
        Spec.Given([])
            .When(() =>
                ShoppingCart.Open(shoppingCartId, clientId, now)
            )
            .Then(new ShoppingCartOpened(shoppingCartId, clientId, now));

    // Add
    [Fact]
    public void CantAddProductItemToNotExistingShoppingCart() =>
        Spec.Given([])
            .When(state =>
                state.AddProduct(
                    FakeProductPriceCalculator.Returning(Price),
                    ProductItem(),
                    now
                )
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void AddsProductItemToEmptyShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now)
            ])
            .When(state =>
                state.AddProduct(
                    FakeProductPriceCalculator.Returning(Price),
                    ProductItem(),
                    now
                )
            )
            .Then(
                new ProductItemAddedToShoppingCart(
                    shoppingCartId,
                    new PricedProductItem
                    {
                        ProductId = ProductItem().ProductId, Quantity = ProductItem().Quantity, UnitPrice = Price
                    },
                    now
                )
            );


    [Fact]
    public void AddsProductItemToNonEmptyShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, PricedProductItem(), now),
            ])
            .When(state =>
                state.AddProduct(
                    FakeProductPriceCalculator.Returning(OtherPrice),
                    OtherProductItem(),
                    now
                )
            )
            .Then(
                new ProductItemAddedToShoppingCart(
                    shoppingCartId,
                    new PricedProductItem
                    {
                        ProductId = OtherProductItem().ProductId,
                        Quantity = OtherProductItem().Quantity,
                        UnitPrice = OtherPrice
                    },
                    now
                )
            );

    [Fact]
    public void CantAddProductItemToConfirmedShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, PricedProductItem(), now),
                new ShoppingCartConfirmed(shoppingCartId, now)
            ])
            .When(state =>
                state.AddProduct(
                    FakeProductPriceCalculator.Returning(Price),
                    ProductItem(),
                    now
                )
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantAddProductItemToCanceledShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, PricedProductItem(), now),
                new ShoppingCartCanceled(shoppingCartId, now)
            ])
            .When(state =>
                state.AddProduct(
                    FakeProductPriceCalculator.Returning(Price),
                    ProductItem(),
                    now
                )
            )
            .ThenThrows<InvalidOperationException>();

    // Remove
    [Fact]
    public void CantRemoveProductItemFromNotExistingShoppingCart() =>
        Spec.Given([])
            .When(state =>
                state.RemoveProduct(PricedProductItem(), now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void RemovesExistingProductItemFromShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, PricedProductItem(), now),
            ])
            .When(state =>
                state.RemoveProduct(PricedProductItemWithQuantity(1), now)
            )
            .Then(
                new ProductItemRemovedFromShoppingCart(
                    shoppingCartId,
                    PricedProductItemWithQuantity(1),
                    now
                )
            );

    [Fact]
    public void CantRemoveNonExistingProductItemFromNonEmptyShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, OtherPricedProductItem(), now),
            ])
            .When(state =>
                state.RemoveProduct(PricedProductItemWithQuantity(1), now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantRemoveExistingProductItemFromCanceledShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, PricedProductItem(), now),
                new ShoppingCartConfirmed(shoppingCartId, now)
            ])
            .When(state =>
                state.RemoveProduct(PricedProductItemWithQuantity(1), now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantRemoveExistingProductItemFromConfirmedShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, PricedProductItem(), now),
                new ShoppingCartCanceled(shoppingCartId, now)
            ])
            .When(state =>
                state.RemoveProduct(PricedProductItemWithQuantity(1), now)
            )
            .ThenThrows<InvalidOperationException>();

    // Confirm
    [Fact]
    public void CantConfirmNotExistingShoppingCart() =>
        Spec.Given([])
            .When(state =>
                state.Confirm(now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    [Trait("Category", "SkipCI")]
    public void CantConfirmEmptyShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
            ])
            .When(state =>
                state.Confirm(now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void ConfirmsNonEmptyShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, PricedProductItem(), now),
            ])
            .When(state =>
                state.Confirm(now)
            )
            .Then(
                new ShoppingCartConfirmed(shoppingCartId, now)
            );

    [Fact]
    public void CantConfirmAlreadyConfirmedShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, PricedProductItem(), now),
                new ShoppingCartConfirmed(shoppingCartId, now)
            ])
            .When(state =>
                state.Confirm(now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantConfirmCanceledShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, PricedProductItem(), now),
                new ShoppingCartCanceled(shoppingCartId, now)
            ])
            .When(state =>
                state.Confirm(now)
            )
            .ThenThrows<InvalidOperationException>();

    // Cancel
    [Fact]
    [Trait("Category", "SkipCI")]
    public void CantCancelNotExistingShoppingCart() =>
        Spec.Given([])
            .When(state =>
                state.Cancel(now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CancelsEmptyShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
            ])
            .When(state =>
                state.Cancel(now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CancelsNonEmptyShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, PricedProductItem(), now),
            ])
            .When(state =>
                state.Cancel(now)
            )
            .Then(
                new ShoppingCartCanceled(shoppingCartId, now)
            );

    [Fact]
    public void CantCancelAlreadyCanceledShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, PricedProductItem(), now),
                new ShoppingCartCanceled(shoppingCartId, now)
            ])
            .When(state =>
                state.Cancel(now)
            )
            .ThenThrows<InvalidOperationException>();

    [Fact]
    public void CantCancelConfirmedShoppingCart() =>
        Spec.Given([
                new ShoppingCartOpened(shoppingCartId, clientId, now),
                new ProductItemAddedToShoppingCart(shoppingCartId, PricedProductItem(), now),
                new ShoppingCartConfirmed(shoppingCartId, now)
            ])
            .When(state =>
                state.Cancel(now)
            )
            .ThenThrows<InvalidOperationException>();

    private readonly DateTimeOffset now = DateTimeOffset.Now;
    private readonly Guid clientId = Guid.CreateVersion7();
    private readonly Guid shoppingCartId = Guid.CreateVersion7();
    private static readonly Guid ProductId = Guid.CreateVersion7();
    private static readonly Guid OtherProductId = Guid.CreateVersion7();
    private static readonly int Quantity = Random.Shared.Next(1, 1000);
    private static readonly int OtherQuantity = Random.Shared.Next(1, 1000);
    private static readonly int Price = Random.Shared.Next(1, 1000);
    private static readonly int OtherPrice = Random.Shared.Next(1, 1000);

    private static readonly Func<ProductItem> ProductItem = () =>
        new ProductItem { ProductId = ProductId, Quantity = Quantity };

    private static readonly Func<ProductItem> OtherProductItem = () =>
        new ProductItem { ProductId = OtherProductId, Quantity = OtherQuantity };

    private static readonly Func<PricedProductItem> PricedProductItem = () => new PricedProductItem
    {
        ProductId = ProductId, Quantity = Quantity, UnitPrice = Price
    };

    private static readonly Func<int, PricedProductItem> PricedProductItemWithQuantity =
        quantity => new PricedProductItem { ProductId = ProductId, Quantity = quantity, UnitPrice = Price };

    private static readonly Func<PricedProductItem> OtherPricedProductItem = () => new PricedProductItem
    {
        ProductId = OtherProductId, Quantity = OtherQuantity, UnitPrice = Price
    };

    private readonly HandlerSpecification<ShoppingCartEvent, ShoppingCart> Spec =
        Specification.For(Evolve, ShoppingCart.Initial);

    private static readonly Func<ShoppingCart, ShoppingCartEvent, ShoppingCart> Evolve = (cart, @event) =>
    {
        cart.Evolve(@event);
        return cart;
    };
}

public static class TestsExtensions
{
    public static ThenDeciderSpecificationBuilder<TEvent, TState> When<TEvent, TState>(
        this WhenDeciderSpecificationBuilder<Handler<TEvent, TState>, TEvent, TState> builder,
        Func<TState> when
    ) where TState : Aggregate<TEvent> =>
        builder.When(() =>
        {
            var state = when();

            return state.DequeueUncommittedEvents();
        });

    public static ThenDeciderSpecificationBuilder<TEvent, TState> When<TEvent, TState>(
        this WhenDeciderSpecificationBuilder<Handler<TEvent, TState>, TEvent, TState> builder,
        Action<TState> when
    ) where TState : Aggregate<TEvent> =>
        builder.When(state =>
        {
            when(state);

            return state.DequeueUncommittedEvents();
        });
}
