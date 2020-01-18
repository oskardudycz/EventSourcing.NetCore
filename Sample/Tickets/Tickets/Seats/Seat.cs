using System;
using Core.Aggregates;

namespace Tickets.Seats
{
    internal class Seat : Aggregate
    {
        public Guid ConcertId { get; private set; }

        public string Number { get; private set; }

        public decimal Price { get; private set; }
    }
}
