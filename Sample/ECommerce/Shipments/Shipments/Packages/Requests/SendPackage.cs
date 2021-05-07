using System;
using System.Collections.Generic;
using Shipments.Products;

namespace Shipments.Packages.Requests
{
    public record SendPackage(
        Guid OrderId,
        List<ProductItem> ProductItems
    );
}
