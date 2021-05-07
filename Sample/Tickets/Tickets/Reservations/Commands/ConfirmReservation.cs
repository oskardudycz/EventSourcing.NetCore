using System;
using Core.Commands;

namespace Tickets.Reservations.Commands
{
    public class ConfirmReservation : ICommand
    {
        public Guid ReservationId { get; }

        private ConfirmReservation(Guid reservationId)
        {
            ReservationId = reservationId;
        }

        public static ConfirmReservation Create(Guid? reservationId)
        {
            if (!reservationId.HasValue)
                throw new ArgumentNullException(nameof(reservationId));

            return new ConfirmReservation(reservationId.Value);
        }
    }
}
