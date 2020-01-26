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

        public void Apply(TentativeReservationCreated @event)
        {
            Id = @event.ReservationId;
            Number = @event.Number;
            Status = ReservationStatus.Tentative;
        }

        public void Apply(ReservationConfirmed @event)
        {
            Status = ReservationStatus.Confirmed;
        }
    }

    internal class ReservationShortInfoProjection : ViewProjection<ReservationShortInfo, Guid>
    {
        public ReservationShortInfoProjection()
        {
            ProjectEvent<TentativeReservationCreated>(@event => @event.ReservationId,
                (item, @event) => item.Apply(@event));

            ProjectEvent<ReservationConfirmed>(@event => @event.ReservationId,
                (item, @event) => item.Apply(@event));


            DeleteEvent<ReservationCancelled>(@event => @event.ReservationId);
        }
    }
}
