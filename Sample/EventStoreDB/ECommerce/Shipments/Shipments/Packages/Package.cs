using System;
using System.Collections.Generic;
using Shipments.Products;

namespace Shipments.Packages
{
    public class Package
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }

        public List<ProductItem> ProductItems { get; set; } = default!;

        public DateTime SentAt { get; set; }
    }
}
