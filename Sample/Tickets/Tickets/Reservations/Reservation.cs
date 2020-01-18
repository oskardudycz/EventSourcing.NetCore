using System;
using Core.Aggregates;

namespace Tickets.Reservations
{
    internal class Reservation : Aggregate
    {
        public Guid SeatId { get; private set; }

        public string Number { get; private set; }
    }
}
