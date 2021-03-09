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

    public class ReservationHistoryTransformation : EventProjection
    {
        public ReservationHistory Transform(IEvent<TentativeReservationCreated> input)
        {
            return new ReservationHistory
            {
                Id = Guid.NewGuid(),
                ReservationId = input.Data.ReservationId,
                Description = $"Created tentative reservation with number {input.Data.Number}"
            };
        }

        public ReservationHistory Transform(IEvent<ReservationSeatChanged> input)
        {
            return new ReservationHistory
            {
                Id = Guid.NewGuid(),
                ReservationId = input.Data.ReservationId,
                Description = $"Updated reservation seat to {input.Data.SeatId}"
            };
        }

        public ReservationHistory Transform(IEvent<ReservationConfirmed> input)
        {
            return new ReservationHistory
            {
                Id = Guid.NewGuid(),
                ReservationId = input.Data.ReservationId,
                Description = "Confirmed Reservation"
            };
        }

        public ReservationHistory Transform(IEvent<ReservationCancelled> input)
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
