using System;
using Tickets.Reservations;
using Tickets.Reservations.NumberGeneration;

namespace Tickets.Tests.Stubs.Reservations
{
    internal class FakeReservationNumberGenerator: IReservationNumberGenerator
    {
        public string LastGeneratedNumber { get; private set; } = Guid.NewGuid().ToString();
        public string Next() => LastGeneratedNumber = Guid.NewGuid().ToString();
    }
}
