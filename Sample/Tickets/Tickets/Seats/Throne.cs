using Core.Aggregates;

namespace Tickets.Seats;

public class Throne : Seat
{
    public Throne(Guid concertId, string number, decimal price): base(concertId, number, price)
    {
    }
}
