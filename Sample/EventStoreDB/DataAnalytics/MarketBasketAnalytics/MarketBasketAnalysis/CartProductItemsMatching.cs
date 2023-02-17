using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketBasketAnalytics.Carts;
using MarketBasketAnalytics.Carts.ProductItems;

namespace MarketBasketAnalytics.MarketBasketAnalysis
{
    public record CartProductItemsMatched(
        Guid ProductId,
        IReadOnlyList<Guid> RelatedProducts
    );

    public static class CartProductItemsMatching
    {
        public static async Task<IReadOnlyList<CartProductItemsMatched>> Handle(
            Func<Func<Dictionary<Guid, int>, object, Dictionary<Guid, int>>, string, CancellationToken, Task<Dictionary<Guid, int>?>> aggregateStream,
            ShoppingCartConfirmed @event,
            CancellationToken ct
        )
        {
            var productItems = await aggregateStream(
                When,
                ShoppingCart.ToStreamId(@event.ShoppingCartId),
                ct
            );

            var productIds = productItems!.Keys;

            return productIds
                .Select(productId =>
                    new CartProductItemsMatched(
                        productId,
                        productIds.Where(pid => pid != productId).ToList()
                    )
                )
                .ToList();
        }

        public static Dictionary<Guid, int> When(Dictionary<Guid, int>? productItems, object @event) =>
            @event switch
            {
                ShoppingCartInitialized =>
                    new Dictionary<Guid, int>(),

                ProductItemAddedToShoppingCart (_, var productItem) =>
                    Add(
                        productItems!,
                        productItem
                    ),

                ProductItemRemovedFromShoppingCart (_, var productItem) =>
                    Subtract(
                        productItems!,
                        productItem
                    ),

                _ => productItems!
            };

        private static Dictionary<Guid, int> Add(Dictionary<Guid, int> productItems, PricedProductItem productItem)
        {
            var productId = productItem.ProductId;
            var quantity = productItem.Quantity;

            var result = new Dictionary<Guid, int>(productItems);
            if (!productItems.ContainsKey(productId))
            {
                result.Add(productId, quantity);
                return result;
            }

            result[productId] += quantity;

            return result;
        }

        private static Dictionary<Guid, int> Subtract(Dictionary<Guid, int> productItems, PricedProductItem productItem)
        {
            var productId = productItem.ProductId;
            var quantity = productItem.Quantity;

            var result = new Dictionary<Guid, int>(productItems);

            result[productId] -= quantity;

            if (result[productId] == 0)
                result.Remove(productId);

            return result;
        }

        public static string ToStreamId(Guid shoppingCartId) =>
            $"cart_product_items_matching-{shoppingCartId}";
    }
}
