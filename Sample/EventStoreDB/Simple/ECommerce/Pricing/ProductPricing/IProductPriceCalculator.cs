using ECommerce.ShoppingCarts.ProductItems;

namespace ECommerce.Pricing.ProductPricing
{
    public interface IProductPriceCalculator
    {
        PricedProductItem Calculate(ProductItem productItem);
    }
}
