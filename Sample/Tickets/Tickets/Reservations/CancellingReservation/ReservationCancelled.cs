using System;
using Core.Events;

namespace Tickets.Reservations.CancellingReservation;

public record ReservationCancelled(
    Guid ReservationId
): IEvent;
