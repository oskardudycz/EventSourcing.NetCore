using System;
using MarketBasketAnalytics.Carts.ProductItems;

namespace MarketBasketAnalytics.Carts
{
    public record ShoppingCartInitialized(
        Guid ShoppingCartId,
        Guid ClientId,
        DateTime InitializedAt
    );

    public record ProductItemAddedToShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem
    );

    public record ProductItemRemovedFromShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem
    );

    public record ShoppingCartConfirmed(
        Guid ShoppingCartId,
        DateTime ConfirmedAt
    );

    public record ShoppingCartAbandoned(
        Guid ShoppingCartId,
        DateTime AbandonedAt
    );

    public static class ShoppingCart
    {
        public static string ToStreamId(Guid shoppingCartId) =>
            $"shopping_cart-{shoppingCartId}";
    }
}
