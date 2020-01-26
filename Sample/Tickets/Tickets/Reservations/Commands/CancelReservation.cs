using System;
using Ardalis.GuardClauses;
using Core.Commands;

namespace Tickets.Reservations.Events
{
    public class CancelReservation : ICommand
    {
        public Guid ReservationId { get; }

        private CancelReservation(Guid reservationId)
        {
            ReservationId = reservationId;
        }

        public static CancelReservation Create(Guid reservationId)
        {
            Guard.Against.Default(reservationId, nameof(reservationId));

            return new CancelReservation(reservationId);
        }
    }
}
