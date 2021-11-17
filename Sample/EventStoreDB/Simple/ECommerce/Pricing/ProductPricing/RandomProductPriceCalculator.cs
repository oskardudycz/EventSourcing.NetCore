using System;
using System.Collections.Concurrent;
using ECommerce.ShoppingCarts.ProductItems;

namespace ECommerce.Pricing.ProductPricing;

public class RandomProductPriceCalculator: IProductPriceCalculator
{
    private readonly ConcurrentDictionary<Guid, decimal> productPrices = new();

    public PricedProductItem Calculate(ProductItem productItem)
    {
        var random = new Random();

        var price = productPrices.GetOrAdd(
            productItem.ProductId,
            (decimal)random.NextDouble() * 100
        );
        return PricedProductItem.From(productItem, price);
    }
}