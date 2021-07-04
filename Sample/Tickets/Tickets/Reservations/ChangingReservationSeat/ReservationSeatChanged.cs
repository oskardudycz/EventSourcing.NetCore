using System;
using Ardalis.GuardClauses;
using Core.Events;
using Newtonsoft.Json;

namespace Tickets.Reservations.ChangingReservationSeat
{
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
            Guard.Against.Default(reservationId, nameof(reservationId));
            Guard.Against.Default(seatId, nameof(seatId));

            return new ReservationSeatChanged(
                reservationId,
                seatId
            );
        }
    }
}
