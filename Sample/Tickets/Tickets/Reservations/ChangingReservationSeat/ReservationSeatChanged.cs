using System;
using Core.Events;
using Newtonsoft.Json;

namespace Tickets.Reservations.ChangingReservationSeat;

public class ReservationSeatChanged : IEvent
{
    public Guid ReservationId { get; }
    public Guid SeatId { get; }

    [JsonConstructor]
    private ReservationSeatChanged(Guid reservationId, Guid seatId)
    {
        ReservationId = reservationId;
        SeatId = seatId;
    }

    public static ReservationSeatChanged Create(Guid reservationId, Guid seatId)
    {
        if (reservationId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(reservationId));
        if (seatId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(seatId));

        return new ReservationSeatChanged(
            reservationId,
            seatId
        );
    }
}