using System;
using Ardalis.GuardClauses;
using Core.Aggregates;
using Core.Ids;
using Tickets.Reservations.Events;

namespace Tickets.Reservations
{
    internal class Reservation : Aggregate
    {
        public Guid SeatId { get; private set; }

        public string Number { get; private set; }

        public ReservationStatus Status { get; private set; }

        private Reservation() { }

        private Reservation(IAggregateIdGenerator<Reservation> idGenerator, Guid seatId, string number)
        {
            Guard.Against.Null(idGenerator, nameof(idGenerator));
            Guard.Against.Default(seatId, nameof(seatId));
            Guard.Against.NullOrWhiteSpace(number, nameof(number));

            var id = idGenerator.New();

            var @event = TentativeReservationCreated.Create(
                id,
                seatId,
                number
            );

            Enqueue(@event);
            Apply(@event);
        }

        public static Reservation CreateTentative(IAggregateIdGenerator<Reservation> idGenerator, Guid seatId, string number)
        {
            return new Reservation(idGenerator, seatId, number);
        }

        private void Apply(TentativeReservationCreated @event)
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

        private void Apply(ReservationSeatChanged @event)
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

        private void Apply(ReservationConfirmed @event)
        {
            Status = ReservationStatus.Cancelled;
            Version++;
        }

        public void Cancel()
        {
            if(Status == ReservationStatus.Tentative)
                throw new InvalidOperationException($"Only tentative reservation can be cancelled (current status: {Status}.");

            var @event = ReservationCancelled.Create(Id);

            Enqueue(@event);
            Apply(@event);
        }

        private void Apply(ReservationCancelled @event)
        {
            Status = ReservationStatus.Cancelled;
            Version++;
        }
    }
}
