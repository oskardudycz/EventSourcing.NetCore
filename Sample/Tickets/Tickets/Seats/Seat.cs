using Core.Aggregates;

namespace Tickets.Seats;

public class Seat(Guid concertId, string number, decimal price): Aggregate
{
    public Guid ConcertId { get; private set; } = concertId;

    public string Number { get; private set; } = number;

    public decimal Price { get; private set; } = price;
}
