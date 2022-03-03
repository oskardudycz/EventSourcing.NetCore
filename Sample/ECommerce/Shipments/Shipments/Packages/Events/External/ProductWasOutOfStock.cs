using Core.Events;

namespace Shipments.Packages.Events.External;

internal class ProductWasOutOfStock: IExternalEvent
{
    public Guid OrderId { get; }

    public DateTime AvailabilityCheckedAt { get; }


    public ProductWasOutOfStock(Guid orderId, DateTime availabilityCheckedAt)
    {
        OrderId = orderId;
        AvailabilityCheckedAt = availabilityCheckedAt;
    }
}