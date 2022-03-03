using Core.Events;
using Orders.Products;

namespace Orders.Shipments.SendingPackage;

public class PackageWasSent : IExternalEvent
{
    public Guid PackageId { get; }
    public Guid OrderId { get; }

    public IReadOnlyList<ProductItem> ProductItems { get; }

    public DateTime SentAt { get; }

    public PackageWasSent(Guid packageId, Guid orderId, IReadOnlyList<ProductItem> productItems, DateTime sentAt)
    {
        OrderId = orderId;
        ProductItems = productItems;
        SentAt = sentAt;
        PackageId = packageId;
    }
}