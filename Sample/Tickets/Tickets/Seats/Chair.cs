using Core.Aggregates;

namespace Tickets.Seats;

public class Chair : Aggregate
{
    public Guid ConcertId { get; private set; }

    public string Number { get; private set; }

    public decimal Price { get; private set; }

    public Chair(Guid concertId, string number, decimal price)
    {
        ConcertId = concertId;
        Number = number;
        Price = price;
    }
}
