using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using Carts.Carts.Events;
using Carts.Carts.ValueObjects;
using Carts.Pricing;
using Core.Extensions;
using Marten.Events.Projections;

namespace Carts.Carts.Projections
{
    public class CartDetails
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }

        public CartStatus Status { get; set; }

        public IList<PricedProductItem> ProductItems { get; set; }

        public decimal TotalPrice => ProductItems.Sum(pi => pi.TotalPrice);

        public int Version { get; set; }
    }
}
