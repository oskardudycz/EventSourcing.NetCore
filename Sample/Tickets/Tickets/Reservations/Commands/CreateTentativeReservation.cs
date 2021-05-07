using System;
using Core.Commands;

namespace Tickets.Reservations.Commands
{
    public class CreateTentativeReservation : ICommand
    {
        public Guid ReservationId { get; }
        public Guid SeatId { get; }

        private CreateTentativeReservation(Guid reservationId, Guid seatId)
        {
            ReservationId = reservationId;
            SeatId = seatId;
        }

        public static CreateTentativeReservation Create(Guid? reservationId, Guid? seatId)
        {
            if (!reservationId.HasValue)
                throw new ArgumentNullException(nameof(reservationId));
            if (!seatId.HasValue)
                throw new ArgumentNullException(nameof(seatId));

            return new CreateTentativeReservation(reservationId.Value, seatId.Value);
        }
    }
}
