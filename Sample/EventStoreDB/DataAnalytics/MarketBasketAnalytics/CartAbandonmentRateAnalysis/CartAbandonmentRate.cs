using MarketBasketAnalytics.Carts;

namespace MarketBasketAnalytics.CartAbandonmentRateAnalysis
{
    // See pt 5 of https://www.bolt.com/resources/ecommerce-metrics/#
    public record CartAbandonmentRateCalculated(
        Guid ShoppingCartId,
        Guid ClientId,
        int ProductItemsCount,
        decimal TotalAmount,
        DateTime InitializedAt,
        DateTime AbandonedAt,
        TimeSpan TotalTime
    );

    public class CartAbandonmentRate
    {
        public static Task<CartAbandonmentRateCalculated> Handle(
            Func<Func<CartAbandonmentRateCalculated?, object, CartAbandonmentRateCalculated>, string, CancellationToken,
                Task<CartAbandonmentRateCalculated?>> aggregateStream,
            ShoppingCartAbandoned @event,
            CancellationToken ct
        ) =>
            aggregateStream(Evolve, ShoppingCart.ToStreamId(@event.ShoppingCartId), ct)!;

        public static CartAbandonmentRateCalculated Evolve(CartAbandonmentRateCalculated? lastEvent, object @event) =>
            @event switch
            {
                ShoppingCartInitialized (var cartId, var clientId, var initializedAt) =>
                    new CartAbandonmentRateCalculated(cartId, clientId, 0, 0, initializedAt, default, TimeSpan.Zero),

                ProductItemAddedToShoppingCart (_, var productItem) =>
                    lastEvent! with
                    {
                        ProductItemsCount = lastEvent.ProductItemsCount + 1,
                        TotalAmount = lastEvent.TotalAmount + productItem.TotalPrice
                    },

                ProductItemRemovedFromShoppingCart (_, var productItem) =>
                    lastEvent! with
                    {
                        ProductItemsCount = lastEvent.ProductItemsCount - 1,
                        TotalAmount = lastEvent.TotalAmount - productItem.TotalPrice
                    },

                ShoppingCartAbandoned (_, var abandonedAt) =>
                    lastEvent! with
                    {
                        AbandonedAt = abandonedAt,
                        TotalTime = abandonedAt - lastEvent.InitializedAt
                    },
                _ => lastEvent!
            };

        public static string ToStreamId(Guid shoppingCartId) =>
            $"cart_abandonment_rate-{shoppingCartId}";
    }
}
