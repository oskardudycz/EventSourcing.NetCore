using Core.Aggregates;
using Tickets.Reservations.CancellingReservation;
using Tickets.Reservations.ChangingReservationSeat;
using Tickets.Reservations.ConfirmingReservation;
using Tickets.Reservations.CreatingTentativeReservation;
using Tickets.Reservations.NumberGeneration;

namespace Tickets.Reservations;

public class Reservation : Aggregate
{
    public Guid SeatId { get; private set; }

    public string Number { get; private set; } = null!;

    public ReservationStatus Status { get; private set; }

    // For serialization
    public Reservation() { }

    public static Reservation CreateTentative(
        Guid id,
        IReservationNumberGenerator numberGenerator,
        Guid seatId) =>
        new(
            id,
            numberGenerator,
            seatId);

    private Reservation(
        Guid id,
        IReservationNumberGenerator numberGenerator,
        Guid seatId)
    {
        if (id == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(id));
        if (seatId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(seatId));

        var reservationNumber = numberGenerator.Next();

        var @event = TentativeReservationCreated.Create(
            id,
            seatId,
            reservationNumber
        );

        Enqueue(@event);
        Apply(@event);
    }


    public void ChangeSeat(Guid newSeatId)
    {
        if (newSeatId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(newSeatId));

        if(Status != ReservationStatus.Tentative)
            throw new InvalidOperationException($"Changing seat for the reservation in '{Status}' status is not allowed.");

        var @event = new ReservationSeatChanged(Id, newSeatId);

        Enqueue(@event);
        Apply(@event);
    }

    public void Confirm()
    {
        if(Status != ReservationStatus.Tentative)
            throw new InvalidOperationException($"Only tentative reservation can be confirmed (current status: {Status}.");

        var @event = new ReservationConfirmed(Id);

        Enqueue(@event);
        Apply(@event);
    }

    public void Cancel()
    {
        if(Status != ReservationStatus.Tentative)
            throw new InvalidOperationException($"Only tentative reservation can be cancelled (current status: {Status}).");

        var @event = new ReservationCancelled(Id);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(TentativeReservationCreated @event)
    {
        Id = @event.ReservationId;
        SeatId = @event.SeatId;
        Number = @event.Number;
        Status = ReservationStatus.Tentative;
    }

    public void Apply(ReservationSeatChanged @event)
    {
        SeatId = @event.SeatId;
    }

    public void Apply(ReservationConfirmed @event)
    {
        Status = ReservationStatus.Confirmed;
    }

    public void Apply(ReservationCancelled @event)
    {
        Status = ReservationStatus.Cancelled;
    }
}
