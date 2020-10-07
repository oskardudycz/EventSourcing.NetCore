using System;
using System.Collections.Generic;
using Carts.Carts.ValueObjects;

namespace Shipments.Packages.Commands
{
    public class SentPackage
    {
        public Guid OrderId { get; set; }

        public List<ProductItem> ProductItems { get; set; }
    }
}
