using System;
using System.Collections.Generic;
using Orders.Products.ValueObjects;

namespace Orders.Api.Requests.Carts
{
    public class InitOrderRequest
    {
        public Guid ClientId { get; set; }

        public IReadOnlyList<PricedProductItemRequest> ProductItems { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
