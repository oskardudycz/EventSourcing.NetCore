using System;
using Carts.Carts.Events;
using Marten.Events;
using Marten.Events.Projections;

namespace Carts.Carts.Projections
{
    public class CartHistory
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public string Description { get; set; }
    }
}
