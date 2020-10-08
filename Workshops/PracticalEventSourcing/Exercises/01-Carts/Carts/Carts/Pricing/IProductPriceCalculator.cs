using System;
using System.Collections.Generic;
using Carts.Carts.ValueObjects;

namespace Carts.Pricing
{
    public interface IProductPriceCalculator
    {
        IReadOnlyList<PricedProductItem> Calculate(params ProductItem[] productItems);
    }
}
