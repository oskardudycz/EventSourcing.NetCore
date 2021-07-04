using System;
using Core.Events;
using Newtonsoft.Json;

namespace Tickets.Reservations.CancellingReservation
{
    public class ReservationCancelled : IEvent
    {
        public Guid ReservationId { get; }

        [JsonConstructor]
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
