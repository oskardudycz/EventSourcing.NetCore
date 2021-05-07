using System;
using Core.Commands;

namespace Tickets.Reservations.Commands
{
    public class CancelReservation : ICommand
    {
        public Guid ReservationId { get; }

        private CancelReservation(Guid reservationId)
        {
            ReservationId = reservationId;
        }

        public static CancelReservation Create(Guid? reservationId)
        {
            if (!reservationId.HasValue)
                throw new ArgumentNullException(nameof(reservationId));

            return new CancelReservation(reservationId.Value);
        }
    }
}
