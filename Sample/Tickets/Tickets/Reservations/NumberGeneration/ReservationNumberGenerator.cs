namespace Tickets.Reservations.NumberGeneration;

public interface IReservationNumberGenerator
{
    string Next();
}

internal class ReservationNumberGenerator: IReservationNumberGenerator
{
    public string Next() => Guid.CreateVersion7().ToString();
}
