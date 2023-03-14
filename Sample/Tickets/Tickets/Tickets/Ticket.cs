using Core.Aggregates;

namespace Tickets.Tickets;

public class Ticket : Aggregate
{
    public Guid SeatId { get; }

    public string Number { get; }

    public Ticket(Guid seatId, string number)
    {
        SeatId = seatId;
        Number = number;
    }
}
