using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using Carts.Carts.Products;

namespace Carts.Pricing
{
    public class RandomProductPriceCalculator: IProductPriceCalculator
    {
        public IReadOnlyList<PricedProductItem> Calculate(params ProductItem[] productItems)
        {
            Guard.Against.Zero(productItems.Length, "Product items count");

            var random = new Random();

            return productItems
                .Select(pi =>
                    PricedProductItem.Create(pi, (decimal)random.NextDouble() * 100))
                .ToList();
        }
    }
}
