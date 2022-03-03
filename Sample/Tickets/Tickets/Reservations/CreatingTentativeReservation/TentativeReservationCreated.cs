using Core.Events;

namespace Tickets.Reservations.CreatingTentativeReservation;

public record TentativeReservationCreated(
    Guid ReservationId,
    Guid SeatId,
    string Number
): IEvent
{
    public static TentativeReservationCreated Create(Guid reservationId, Guid seatId, string number)
    {
        if (reservationId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(reservationId));
        if (seatId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(seatId));
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentOutOfRangeException(nameof(number));

        return new TentativeReservationCreated(reservationId, seatId, number);
    }
}
