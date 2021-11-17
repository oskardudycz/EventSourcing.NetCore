using System;
using Core.Events;
using Newtonsoft.Json;

namespace Tickets.Reservations.CreatingTentativeReservation;

public class TentativeReservationCreated: IEvent
{
    public Guid ReservationId { get; }
    public Guid SeatId { get; }
    public string Number { get; }

    [JsonConstructor]
    private TentativeReservationCreated(Guid reservationId, Guid seatId, string number)
    {
        ReservationId = reservationId;
        SeatId = seatId;
        Number = number;
    }

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