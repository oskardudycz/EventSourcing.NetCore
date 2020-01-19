using System;
using Core.Events;

namespace Tickets.Reservations.Events
{
    public class ReservationCancelled : IEvent
    {
        public Guid ReservationId { get; }

        private ReservationCancelled(Guid reservationId)
        {
            ReservationId = reservationId;
        }

        public static ReservationCancelled Create(Guid reservationId)
        {
            return new ReservationCancelled(reservationId);
        }
    }
}
