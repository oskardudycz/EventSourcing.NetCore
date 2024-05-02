using Core.Events;
using Shipments.Products;

namespace Shipments.Packages.Events.External;

internal class PackageWasSent(Guid packageId, Guid orderId, IReadOnlyList<ProductItem> productItems, DateTime sentAt)
    : IExternalEvent
{

    public Guid PackageId { get; } = packageId;
    public Guid OrderId { get; } = orderId;

    public IReadOnlyList<ProductItem> ProductItems { get; } = productItems;

    public DateTime SentAt { get; } = sentAt;
}
