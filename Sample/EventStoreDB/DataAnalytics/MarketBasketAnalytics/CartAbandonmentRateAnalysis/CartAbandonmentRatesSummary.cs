using MarketBasketAnalytics.Carts;

namespace MarketBasketAnalytics.CartAbandonmentRateAnalysis
{
    /// <summary>
    /// The cart abandonment rate is the percent of customers that add an item to cart and then abandon the purchase.
    /// The number of items added to the cart, the value of items added to cart, and how long they shop are all important as well.
    /// </summary>
    /// <param name="TotalCount"></param>
    /// <param name="ConfirmedCount"></param>
    /// <param name="AbandonedCount"></param>
    /// <param name="AbandonmentRate"></param>
    /// <param name="AbandonedTotalTimeInSeconds"></param>
    /// <param name="AverageTime"></param>
    /// <param name="AbandonedProductItemsTotalCount"></param>
    /// <param name="AbandonedProductItemsAverageCount"></param>
    /// <param name="AbandonedTotalAmount"></param>
    /// <param name="AbandonedAverageAmount"></param>
    public record CartAbandonmentRatesSummary(
        int TotalCount,
        int ConfirmedCount,
        int AbandonedCount,
        decimal AbandonmentRate,
        long AbandonedTotalTimeInSeconds,
        TimeSpan AverageTime,
        long AbandonedProductItemsTotalCount,
        decimal AbandonedProductItemsAverageCount,
        decimal AbandonedTotalAmount,
        decimal AbandonedAverageAmount
    )
    {
        public static CartAbandonmentRatesSummary Default() =>
            new(default, default, default, default, default, default, default, default, default, default);

        public static async Task<CartAbandonmentRatesSummary> Handle(
            Func<CancellationToken, Task<CartAbandonmentRatesSummary?>> getCurrentSummary,
            CartAbandonmentRateCalculated @event,
            CancellationToken ct
        )
        {
            var currentSummary = await getCurrentSummary(ct) ?? Default();

            var totalCount = currentSummary.TotalCount + 1;
            var abandonedCount = currentSummary.AbandonedCount + 1;

            var abandonedTotalTime = currentSummary.AbandonedTotalTimeInSeconds + @event.TotalTime.TotalSeconds;
            var abandonedAverageTime = TimeSpan.FromSeconds(abandonedTotalTime / abandonedCount);

            var abandonedProductItemsTotalCount =
                currentSummary.AbandonedProductItemsTotalCount + @event.ProductItemsCount;
            var abandonedProductItemsAverageCount =
                (decimal)abandonedProductItemsTotalCount / abandonedCount;

            var abandonedTotalAmount =
                currentSummary.AbandonedTotalAmount + @event.TotalAmount;
            var abandonedAverageAmount =
                abandonedTotalAmount / abandonedCount;

            return new CartAbandonmentRatesSummary(
                totalCount,
                currentSummary.ConfirmedCount,
                abandonedCount,
                abandonedCount / (decimal)totalCount,
                (long)abandonedTotalTime,
                abandonedAverageTime,
                abandonedProductItemsTotalCount,
                abandonedProductItemsAverageCount,
                abandonedTotalAmount,
                abandonedAverageAmount
            );
        }

        public static async Task<CartAbandonmentRatesSummary> Handle(
            Func<CancellationToken, Task<CartAbandonmentRatesSummary?>> getCurrentSummary,
            ShoppingCartConfirmed @event,
            CancellationToken ct
        )
        {
            var currentSummary = await getCurrentSummary(ct) ?? Default();

            var totalCount = currentSummary.TotalCount + 1;

            return currentSummary with
            {
                TotalCount = totalCount,
                ConfirmedCount = currentSummary.ConfirmedCount + 1,
                AbandonmentRate = currentSummary.AbandonedCount / (decimal)totalCount,
            };
        }

        public const string SummaryId = "cart_abandonment_rates_summary";
    }
}
