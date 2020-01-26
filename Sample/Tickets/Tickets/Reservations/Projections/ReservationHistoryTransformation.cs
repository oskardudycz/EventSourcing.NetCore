using System;
using Marten.Events;
using Marten.Events.Projections;
using Tickets.Reservations.Events;

namespace Tickets.Reservations.Projections
{
    public class ReservationHistory
    {
        public Guid Id { get; set; }
        public Guid ReservationId { get; set; }
        public string Description { get; set; }
    }

    public class ReservationHistoryTransformation :
        ITransform<TentativeReservationCreated, ReservationHistory>,
        ITransform<ReservationSeatChanged, ReservationHistory>,
        ITransform<ReservationConfirmed, ReservationHistory>,
        ITransform<ReservationCancelled, ReservationHistory>
    {
        public ReservationHistory Transform(EventStream stream, Event<TentativeReservationCreated> input)
        {
            return new ReservationHistory
            {
                Id = Guid.NewGuid(),
                ReservationId = input.Data.ReservationId,
                Description = $"Created tentative reservation with number {input.Data.Number}"
            };
        }

        public ReservationHistory Transform(EventStream stream, Event<ReservationSeatChanged> input)
        {
            return new ReservationHistory
            {
                Id = Guid.NewGuid(),
                ReservationId = input.Data.ReservationId,
                Description = $"Updated reservation seat to {input.Data.SeatId}"
            };
        }

        public ReservationHistory Transform(EventStream stream, Event<ReservationConfirmed> input)
        {
            return new ReservationHistory
            {
                Id = Guid.NewGuid(),
                ReservationId = input.Data.ReservationId,
                Description = "Confirmed Reservation"
            };
        }

        public ReservationHistory Transform(EventStream stream, Event<ReservationCancelled> input)
        {
            return new ReservationHistory
            {
                Id = Guid.NewGuid(),
                ReservationId = input.Data.ReservationId,
                Description = "Cancelled Reservation"
            };
        }
    }
}
