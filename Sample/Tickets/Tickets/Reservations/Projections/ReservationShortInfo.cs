using System;
using Marten.Events.Projections;
using Tickets.Reservations.Events;

namespace Tickets.Reservations.Projections
{
    public class ReservationShortInfo
    {
        public Guid Id { get; set; }

        public string Number { get; set; }

        public ReservationStatus Status { get; set; }
    }

    internal class ReservationShortInfoProjection : ViewProjection<ReservationShortInfo, Guid>
    {
        public ReservationShortInfoProjection()
        {
            ProjectEvent<TentativeReservationCreated>(@event => @event.ReservationId, Apply);
        }

        private void Apply(ReservationShortInfo item, TentativeReservationCreated @event)
        {
            item.Id = @event.ReservationId;
            item.Number = @event.Number;
            item.Status = ReservationStatus.Tentative;
        }
    }
}
