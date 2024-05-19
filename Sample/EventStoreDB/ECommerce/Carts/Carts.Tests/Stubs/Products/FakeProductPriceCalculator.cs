using Carts.Pricing;
using Carts.ShoppingCarts.Products;

namespace Carts.Tests.Stubs.Products;

internal class FakeProductPriceCalculator: IProductPriceCalculator
{
    public const decimal FakePrice = 13;
    public IReadOnlyList<PricedProductItem> Calculate(params ProductItem[] productItems) =>
        productItems
            .Select(pi =>
                PricedProductItem.From(pi, FakePrice))
            .ToList();
}
