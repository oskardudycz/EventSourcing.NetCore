using System;
using System.Collections.Generic;
using Carts.Carts.ValueObjects;
using Core.Events;

namespace Shipments.Packages.Events.External
{
    public class ProductWasOutOfStock: IEvent
    {
        public Guid OrderId { get; }

        public IReadOnlyList<ProductItem> AvailableProductItems { get; }

        public DateTime AvailabilityCheckedAt { get; }


        public ProductWasOutOfStock(Guid orderId, IReadOnlyList<ProductItem> availableProductItems, DateTime availabilityCheckedAt)
        {
            OrderId = orderId;
            AvailableProductItems = availableProductItems;
            AvailabilityCheckedAt = availabilityCheckedAt;
        }
    }
}
