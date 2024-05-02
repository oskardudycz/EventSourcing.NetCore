using Core.Aggregates;

namespace Tickets.Tickets;

public class Ticket(Guid seatId, string number): Aggregate
{
    public Guid SeatId { get; private set; } = seatId;

    public string Number { get; private set; } = number;
}
