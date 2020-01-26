using System;
using Ardalis.GuardClauses;
using Core.Commands;

namespace Tickets.Reservations.Events
{
    public class ChangeReservationSeat : ICommand
    {
        public Guid ReservationId { get; }
        public Guid SeatId { get; }

        private ChangeReservationSeat(Guid reservationId, Guid seatId)
        {
            ReservationId = reservationId;
            SeatId = seatId;
        }

        public static ChangeReservationSeat Create(Guid reservationId, Guid seatId)
        {
            Guard.Against.Default(reservationId, nameof(reservationId));
            Guard.Against.Default(seatId, nameof(seatId));

            return new ChangeReservationSeat(
                reservationId,
                seatId
            );
        }
    }
}
