using System;
using Ardalis.GuardClauses;
using Core.Aggregates;
using Tickets.Reservations.Events;

namespace Tickets.Reservations
{
    internal class Reservation : Aggregate
    {
        public Guid SeatId { get; private set; }

        public string Number { get; private set; }

        public ReservationStatus Status { get; private set; }

        // For serialization
        public Reservation() { }

        private Reservation(
            Guid id,
            IReservationNumberGenerator numberGenerator,
            Guid seatId)
        {
            Guard.Against.Null(id, nameof(id));
            Guard.Against.Null(numberGenerator, nameof(numberGenerator));
            Guard.Against.Default(seatId, nameof(seatId));

            var reservationNumber = numberGenerator.Next();

            Guard.Against.Default(id, nameof(id));
            Guard.Against.NullOrWhiteSpace(reservationNumber, nameof(reservationNumber));

            var @event = TentativeReservationCreated.Create(
                id,
                seatId,
                reservationNumber
            );

            Enqueue(@event);
            Apply(@event);
        }

        public static Reservation CreateTentative(
            Guid id,
            IReservationNumberGenerator numberGenerator,
            Guid seatId)
        {
            return new Reservation(
                id,
                numberGenerator,
                seatId);
        }

        public void Apply(TentativeReservationCreated @event)
        {
            Id = @event.ReservationId;
            SeatId = @event.SeatId;
            Number = @event.Number;
            Status = ReservationStatus.Tentative;
            Version++;
        }

        public void ChangeSeat(Guid newSeatId)
        {
            Guard.Against.Default(newSeatId, nameof(newSeatId));

            if(Status != ReservationStatus.Tentative)
                throw new InvalidOperationException($"Changing seat for the reservation in '{Status}' status is not allowed.");

            var @event = ReservationSeatChanged.Create(Id, newSeatId);

            Enqueue(@event);
            Apply(@event);
        }

        public void Apply(ReservationSeatChanged @event)
        {
            SeatId = @event.SeatId;
            Version++;
        }

        public void Confirm()
        {
            if(Status != ReservationStatus.Tentative)
                throw new InvalidOperationException($"Only tentative reservation can be confirmed (current status: {Status}.");

            var @event = ReservationConfirmed.Create(Id);

            Enqueue(@event);
            Apply(@event);
        }

        public void Apply(ReservationConfirmed @event)
        {
            Status = ReservationStatus.Confirmed;
            Version++;
        }

        public void Cancel()
        {
            if(Status != ReservationStatus.Tentative)
                throw new InvalidOperationException($"Only tentative reservation can be cancelled (current status: {Status}).");

            var @event = ReservationCancelled.Create(Id);

            Enqueue(@event);
            Apply(@event);
        }

        public void Apply(ReservationCancelled @event)
        {
            Status = ReservationStatus.Cancelled;
            Version++;
        }
    }
}
