using MarketBasketAnalytics.CartAbandonmentRateAnalysis;
using MarketBasketAnalytics.MarketBasketAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace MarketBasketAnalytics
{
    public static class Configuration
    {
        public static IServiceCollection AddMarketBasketAnalytics(this IServiceCollection services) =>
            services
                .AddCartAbandonmentRateAnalysis()
                .AddMarketBasketAnalysis();
    }
}
