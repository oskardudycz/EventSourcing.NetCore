using Core.Events;
using Orders.Products;

namespace Orders.Shipments.SendingPackage;

public record PackageWasSent(
    Guid PackageId,
    Guid OrderId,
    IReadOnlyList<ProductItem> ProductItems,
    DateTimeOffset SentAt
): IExternalEvent;
