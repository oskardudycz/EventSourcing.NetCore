using Core.Events;
using Orders.Products;

namespace Orders.Shipments.OutOfStockProduct;

public class ProductWasOutOfStock: IEvent
{
    public Guid OrderId { get; }

    public IReadOnlyList<ProductItem> AvailableProductItems { get; }

    public DateTime AvailabilityCheckedAt { get; }


    public ProductWasOutOfStock(
        Guid orderId,
        IReadOnlyList<ProductItem> availableProductItems,
        DateTime availabilityCheckedAt
    )
    {
        OrderId = orderId;
        AvailableProductItems = availableProductItems;
        AvailabilityCheckedAt = availabilityCheckedAt;
    }
}