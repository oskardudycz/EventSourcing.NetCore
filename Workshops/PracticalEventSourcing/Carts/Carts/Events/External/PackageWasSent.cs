using System;
using System.Collections.Generic;
using Carts.Carts.ValueObjects;
using Core.Events;

namespace Shipments.Packages.Events.External
{
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
}
