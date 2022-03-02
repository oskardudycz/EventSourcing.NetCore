using System.Collections.Generic;
using Carts.ShoppingCarts.Products;

namespace Carts.Pricing;

public interface IProductPriceCalculator
{
    IReadOnlyList<PricedProductItem> Calculate(params ProductItem[] productItems);
}