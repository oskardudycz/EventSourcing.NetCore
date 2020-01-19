using System;
using Tickets.Reservations;

namespace Tickets.Tests.Stubs.Reservations
{
    internal class FakeReservationNumberGenerator: IReservationNumberGenerator
    {
        public string LastGeneratedNumber { get; private set; }
        public string Next() => LastGeneratedNumber = Guid.NewGuid().ToString();
    }
}
