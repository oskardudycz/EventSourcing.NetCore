using System;
using Core.Events;

namespace Tickets.Reservations.ConfirmingReservation;

public record ReservationConfirmed(
    Guid ReservationId
): IEvent;
