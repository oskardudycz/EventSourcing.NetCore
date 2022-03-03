using Core.Events;
using Shipments.Products;

namespace Shipments.Packages.Events.External;

internal class PackageWasSent : IExternalEvent
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