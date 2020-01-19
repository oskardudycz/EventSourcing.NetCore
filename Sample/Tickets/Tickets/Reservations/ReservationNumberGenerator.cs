using System;

namespace Tickets.Reservations
{
    public interface IReservationNumberGenerator
    {
        string Next();
    }

    public class ReservationNumberGenerator: IReservationNumberGenerator
    {
        public string Next() => Guid.NewGuid().ToString();
    }
}
