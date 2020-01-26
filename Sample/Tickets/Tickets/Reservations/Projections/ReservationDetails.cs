using System;
using Marten.Events.Projections;
using Tickets.Reservations.Events;

namespace Tickets.Reservations.Projections
{
    public class ReservationDetails
    {
        public Guid Id { get; set; }

        public string Number { get; set; }

        public string SitNumber { get; set; }

        public ReservationStatus Status { get; set; }
    }

    internal class ReservationDetailsProjection : ViewProjection<ReservationDetails, Guid>
    {
        public ReservationDetailsProjection()
        {
            ProjectEvent<TentativeReservationCreated>(@event => @event.ReservationId, Apply);
        }

        private void Apply(ReservationDetails item, TentativeReservationCreated @event)
        {
            item.Id = @event.ReservationId;
            item.Number = @event.Number;
            item.Status = ReservationStatus.Tentative;
        }
    }
}
