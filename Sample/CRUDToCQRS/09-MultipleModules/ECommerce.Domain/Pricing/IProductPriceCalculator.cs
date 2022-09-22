using ECommerce.Domain.ShoppingCarts.Products;

namespace ECommerce.Domain.Pricing;

public interface IProductPriceCalculator
{
    IReadOnlyList<PricedProductItem> Calculate(params ProductItem[] productItems);
}
