using System;
using System.Collections.Generic;
using Shipments.Products;

namespace Shipments.Packages.Commands
{
    public class SendPackage
    {
        public Guid OrderId { get; set; }

        public List<ProductItem> ProductItems { get; set; }
    }
}
