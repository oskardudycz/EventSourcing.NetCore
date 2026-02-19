using Tickets.Reservations.NumberGeneration;

namespace Tickets.Tests.Stubs.Reservations;

internal class FakeReservationNumberGenerator: IReservationNumberGenerator
{
    public string LastGeneratedNumber { get; private set; } = Guid.CreateVersion7().ToString();
    public string Next() => LastGeneratedNumber = Guid.CreateVersion7().ToString();
}
