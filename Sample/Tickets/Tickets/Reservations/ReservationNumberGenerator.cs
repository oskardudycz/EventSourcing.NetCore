using System;

namespace Tickets.Reservations
{
    internal interface IReservationNumberGenerator
    {
        string Next();
    }

    internal class ReservationNumberGenerator: IReservationNumberGenerator
    {
        public string Next() => Guid.NewGuid().ToString();
    }
}
