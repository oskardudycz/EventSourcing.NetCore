using static Decider.ShoppingCartEvent;

namespace Decider;

public abstract record ShoppingCart
{
    public record EmptyShoppingCart: ShoppingCart;

    public record PendingShoppingCart(
        Guid Id,
        Guid ClientId,
        ProductItems ProductItems
    ): ShoppingCart;

    public record ConfirmedShoppingCart(
        Guid Id,
        Guid ClientId,
        ProductItems ProductItems,
        DateTimeOffset ConfirmedAt
    ): ShoppingCart;

    public record CanceledShoppingCart(
        Guid Id,
        Guid ClientId,
        ProductItems ProductItems,
        DateTimeOffset CanceledAt
    ): ShoppingCart;

    public static ShoppingCart Empty = new EmptyShoppingCart();

    public static ShoppingCart Evolve(ShoppingCart state, ShoppingCartEvent @event) =>
        @event switch
        {
            ShoppingCartOpened (var shoppingCartId, var clientId) =>
                state is EmptyShoppingCart
                    ? new PendingShoppingCart(shoppingCartId, clientId, ProductItems.Empty)
                    : state,
            ProductItemAddedToShoppingCart (_, var pricedProductItem) =>
                state is PendingShoppingCart pendingShoppingCart
                    ? pendingShoppingCart with
                    {
                        ProductItems = pendingShoppingCart.ProductItems.Add(pricedProductItem)
                    }
                    : state,
            ProductItemRemovedFromShoppingCart (_, var pricedProductItem) =>
                state is PendingShoppingCart pendingShoppingCart
                    ? pendingShoppingCart with
                    {
                        ProductItems = pendingShoppingCart.ProductItems.Remove(pricedProductItem)
                    }
                    : state,
            ShoppingCartConfirmed (_, var confirmedAt) =>
                state is PendingShoppingCart (var shoppingCartId, var clientId, var productItems)
                    ? new ConfirmedShoppingCart(shoppingCartId, clientId, productItems, confirmedAt)
                    : state,
            ShoppingCartCanceled (_, var canceledAt) =>
                state is PendingShoppingCart (var shoppingCartId, var clientId, var productItems)
                    ? new CanceledShoppingCart(shoppingCartId, clientId, productItems, canceledAt)
                    : state,
            _ => state
        };

    private ShoppingCart() { }
}

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
        DateTimeOffset ConfirmedAt
    ): ShoppingCartEvent;

    public record ShoppingCartCanceled(
        Guid ShoppingCartId,
        DateTimeOffset CanceledAt
    ): ShoppingCartEvent;

    private ShoppingCartEvent() { }
}
