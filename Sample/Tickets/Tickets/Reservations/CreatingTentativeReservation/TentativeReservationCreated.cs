using System;
using Ardalis.GuardClauses;
using Core.Events;
using Newtonsoft.Json;

namespace Tickets.Reservations.CreatingTentativeReservation
{
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
            Guard.Against.Default(reservationId, nameof(reservationId));
            Guard.Against.Default(seatId, nameof(seatId));
            Guard.Against.NullOrWhiteSpace(number, nameof(number));

            return new TentativeReservationCreated(reservationId, seatId, number);
        }
    }
}
