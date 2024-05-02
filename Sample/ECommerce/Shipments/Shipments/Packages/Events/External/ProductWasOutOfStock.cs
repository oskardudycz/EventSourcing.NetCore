using Core.Events;

namespace Shipments.Packages.Events.External;

internal class ProductWasOutOfStock(Guid orderId, DateTime availabilityCheckedAt): IExternalEvent
{
    public Guid OrderId { get; } = orderId;

    public DateTime AvailabilityCheckedAt { get; } = availabilityCheckedAt;
}
