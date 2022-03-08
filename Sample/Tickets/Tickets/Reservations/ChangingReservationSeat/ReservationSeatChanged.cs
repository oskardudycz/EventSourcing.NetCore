namespace Tickets.Reservations.ChangingReservationSeat;

public record ReservationSeatChanged(
    Guid ReservationId,
    Guid SeatId
);
