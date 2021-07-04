using System;
using Marten.Events.Aggregation;
using Tickets.Reservations.CancellingReservation;
using Tickets.Reservations.ChangingReservationSeat;
using Tickets.Reservations.ConfirmingReservation;
using Tickets.Reservations.CreatingTentativeReservation;

namespace Tickets.Reservations.GettingReservationById
{
    public class ReservationDetails
    {
        public Guid Id { get; set; }

        public string Number { get; set; } = default!;

        public Guid SeatId { get; set; }

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

    public class ReservationDetailsProjection: AggregateProjection<ReservationDetails>
    {
        public ReservationDetailsProjection()
        {
            ProjectEvent<TentativeReservationCreated>((item, @event) => item.Apply(@event));

            ProjectEvent<ReservationSeatChanged>((item, @event) => item.Apply(@event));

            ProjectEvent<ReservationConfirmed>((item, @event) => item.Apply(@event));

            ProjectEvent<ReservationCancelled>((item, @event) => item.Apply(@event));
        }
    }
}
