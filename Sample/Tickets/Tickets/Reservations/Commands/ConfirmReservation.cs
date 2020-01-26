using System;
using Core.Commands;

namespace Tickets.Reservations.Events
{
    public class ConfirmReservation : ICommand
    {
        public Guid ReservationId { get; }

        private ConfirmReservation(Guid reservationId)
        {
            ReservationId = reservationId;
        }

        public static ConfirmReservation Create(Guid reservationId)
        {
            return new ConfirmReservation(reservationId);
        }
    }
}
