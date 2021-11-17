using System.Collections.Generic;
using Carts.Carts.Products;

namespace Carts.Pricing;

public interface IProductPriceCalculator
{
    IReadOnlyList<PricedProductItem> Calculate(params ProductItem[] productItems);
}