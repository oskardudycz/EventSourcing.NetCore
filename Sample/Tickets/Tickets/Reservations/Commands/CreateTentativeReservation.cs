using System;
using Ardalis.GuardClauses;
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

        public static CreateTentativeReservation Create(Guid reservationId, Guid seatId)
        {
            Guard.Against.Default(reservationId, nameof(reservationId));
            Guard.Against.Default(seatId, nameof(seatId));

            return new CreateTentativeReservation(reservationId, seatId);
        }
    }
}
