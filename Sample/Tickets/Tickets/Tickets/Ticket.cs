using System;
using Core.Aggregates;

namespace Tickets.Tickets
{
    public class Ticket : Aggregate
    {
        public Guid SeatId { get; private set; }

        public string Number { get; private set; }
    }
}
