using System;
using System.Collections.Generic;
using Carts.Carts.ValueObjects;

namespace Shipments.Packages
{
    internal class Package
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }

        public List<ProductItem> ProductItems { get; set; }

        public DateTime SentAt { get; set; }
    }
}
