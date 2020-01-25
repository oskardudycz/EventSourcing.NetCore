using System;
using Ardalis.GuardClauses;
using Core.Events;

namespace Tickets.Reservations.Events
{
    public class ReservationSeatChanged : IEvent
    {
        public Guid ReservationId { get; }
        public Guid SeatId { get; }

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
