using System;
using System.Collections.Generic;
using Carts.Carts.ValueObjects;
using Core.Commands;

namespace Shipments.Packages.Commands
{
    public class SentPackage: ICommand
    {
        public Guid OrderId { get; }

        public IReadOnlyList<ProductItem> ProductItems { get; }

        public SentPackage(Guid orderId, IReadOnlyList<ProductItem> productItems)
        {
            OrderId = orderId;
            ProductItems = productItems;
        }
    }
}
