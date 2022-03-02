using System;
using Core.Events;

namespace Tickets.Reservations.ChangingReservationSeat;

public record ReservationSeatChanged(
    Guid ReservationId,
    Guid SeatId
): IEvent;
