using System;
using Marten.Events.Projections;
using Tickets.Reservations.Events;

namespace Tickets.Reservations.Projections
{
    public class ReservationDetails
    {
        public Guid Id { get; set; }

        public string Number { get; set; }

        public Guid SeatId { get; set; }

        public string SeatNumber { get; set; }

        public ReservationStatus Status { get; set; }

        public int Version { get; set; }

        public void Apply(TentativeReservationCreated @event)
        {
            Id = @event.ReservationId;
            SeatId = @event.SeatId;
            Number = @event.Number;
            Status = ReservationStatus.Tentative;
            Version++;
        }

        public void Apply(ReservationSeatChanged @event)
        {
            SeatId = @event.SeatId;
            Version++;
        }

        public void Apply(ReservationConfirmed @event)
        {
            Status = ReservationStatus.Confirmed;
            Version++;
        }

        public void Apply(ReservationCancelled @event)
        {
            Status = ReservationStatus.Cancelled;
            Version++;
        }
    }

    internal class ReservationDetailsProjection: ViewProjection<ReservationDetails, Guid>
    {
        public ReservationDetailsProjection()
        {
            ProjectEvent<TentativeReservationCreated>(@event => @event.ReservationId,
                (item, @event) => item.Apply(@event));


            ProjectEvent<ReservationSeatChanged>(@event => @event.ReservationId,
                (item, @event) => item.Apply(@event));

            ProjectEvent<ReservationConfirmed>(@event => @event.ReservationId,
                (item, @event) => item.Apply(@event));


            ProjectEvent<ReservationCancelled>(@event => @event.ReservationId,
                (item, @event) => item.Apply(@event));
        }
    }
}
