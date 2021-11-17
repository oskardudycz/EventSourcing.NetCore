using System.Collections.Generic;
using System.Linq;
using Carts.Carts.Products;
using Carts.Pricing;

namespace Carts.Tests.Stubs.Products;

internal class FakeProductPriceCalculator: IProductPriceCalculator
{
    public const decimal FakePrice = 13;
    public IReadOnlyList<PricedProductItem> Calculate(params ProductItem[] productItems)
    {
        return productItems
            .Select(pi =>
                PricedProductItem.Create(pi, FakePrice))
            .ToList();
    }
}