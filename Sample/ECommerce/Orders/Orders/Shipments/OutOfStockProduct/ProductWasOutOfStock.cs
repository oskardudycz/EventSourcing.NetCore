using Orders.Products;

namespace Orders.Shipments.OutOfStockProduct;

public record ProductWasOutOfStock(
    Guid OrderId,
    IReadOnlyList<ProductItem> AvailableProductItems,
    DateTime AvailabilityCheckedAt
);
