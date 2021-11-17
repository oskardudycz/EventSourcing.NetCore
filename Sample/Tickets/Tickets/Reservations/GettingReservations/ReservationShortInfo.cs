using System;
using Marten.Events.Aggregation;
using Tickets.Reservations.CancellingReservation;
using Tickets.Reservations.ConfirmingReservation;
using Tickets.Reservations.CreatingTentativeReservation;

namespace Tickets.Reservations.GettingReservations;

public class ReservationShortInfo
{
    public Guid Id { get; set; }

    public string Number { get; set; } = default!;

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

public class ReservationShortInfoProjection : AggregateProjection<ReservationShortInfo>
{
    public ReservationShortInfoProjection()
    {
        ProjectEvent<TentativeReservationCreated>((item, @event) => item.Apply(@event));

        ProjectEvent<ReservationConfirmed>((item, @event) => item.Apply(@event));

        DeleteEvent<ReservationCancelled>();
    }
}