using System.Linq;

namespace Shipments.Products
{
    public interface IProductAvailabilityService
    {
        bool IsEnoughOf(params ProductItem[] productItems);
    }

    public class ProductAvailabilityService: IProductAvailabilityService
    {
        public bool IsEnoughOf(params ProductItem[] productItems)
        {
            return productItems.All(pi => pi.Quantity <= 100);
        }
    }
}
