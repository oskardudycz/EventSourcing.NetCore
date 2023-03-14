using Core.Aggregates;

namespace Tickets.Seats;

public class Stool: Seat
{
    public Stool(Guid concertId, string number, decimal price): base(concertId, number, price)
    {
    }
}
