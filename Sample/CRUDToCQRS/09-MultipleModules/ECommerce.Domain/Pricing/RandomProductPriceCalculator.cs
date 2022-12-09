using ECommerce.Domain.ShoppingCarts.Products;

namespace ECommerce.Domain.Pricing;

public class RandomProductPriceCalculator: IProductPriceCalculator
{
    public IReadOnlyList<PricedProductItem> Calculate(params ProductItem[] productItems)
    {
        if (productItems.Length == 0)
            throw new ArgumentOutOfRangeException(nameof(productItems.Length));

        var random = new Random();

        return productItems
            .Select(pi =>
                PricedProductItem.Create(pi, Math.Round(new decimal(random.NextDouble() * 100),2)))
            .ToList();
    }
}
