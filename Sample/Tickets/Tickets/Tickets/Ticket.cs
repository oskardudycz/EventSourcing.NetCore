using System;
using Core.Aggregates;

namespace Tickets.Tickets
{
    internal class Ticket : Aggregate
    {
        public Guid SeatId { get; private set; }

        public string Number { get; private set; }
    }
}
