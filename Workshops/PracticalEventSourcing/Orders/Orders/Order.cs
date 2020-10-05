using System;
using System.Collections.Generic;
using Carts.Carts.ValueObjects;
using Core.Aggregates;

namespace Orders.Orders
{
    public class Order: Aggregate
    {
        public IReadOnlyList<PricedProductItem> ProductItems { get; private set; }
        public Guid PaymentId { get; private set; }
    }
}
