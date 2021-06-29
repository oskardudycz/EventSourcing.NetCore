using System;
using System.Collections.Generic;
using Core.Events;
using Orders.Products.ValueObjects;

namespace Orders.Shipments.Events
{
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
}
