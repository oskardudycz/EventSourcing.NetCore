using System;
using Core.Commands;

namespace Tickets.Reservations.Commands
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

        public static ChangeReservationSeat Create(Guid? reservationId, Guid? seatId)
        {
            if (!reservationId.HasValue)
                throw new ArgumentNullException(nameof(reservationId));
            if (!seatId.HasValue)
                throw new ArgumentNullException(nameof(seatId));

            return new ChangeReservationSeat(
                reservationId.Value,
                seatId.Value
            );
        }
    }
}
