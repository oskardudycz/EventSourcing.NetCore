using System;
using Marten.Events;
using Marten.Events.Projections;
using Tickets.Reservations.Events;

namespace Tickets.Reservations.Projections
{
    public record ReservationHistory(
        Guid Id,
        Guid ReservationId,
        string Description
    );

    public class ReservationHistoryTransformation: EventProjection
    {
        public ReservationHistory Transform(IEvent<TentativeReservationCreated> input)
        {
            return new(
                Guid.NewGuid(),
                input.Data.ReservationId,
                $"Created tentative reservation with number {input.Data.Number}"
            );
        }

        public ReservationHistory Transform(IEvent<ReservationSeatChanged> input)
        {
            return new(
                Guid.NewGuid(),
                input.Data.ReservationId,
                $"Updated reservation seat to {input.Data.SeatId}"
            );
        }

        public ReservationHistory Transform(IEvent<ReservationConfirmed> input)
        {
            return new(
                Guid.NewGuid(),
                input.Data.ReservationId,
                "Confirmed Reservation"
            );
        }

        public ReservationHistory Transform(IEvent<ReservationCancelled> input)
        {
            return new(
                Guid.NewGuid(),
                input.Data.ReservationId,
                "Cancelled Reservation"
            );
        }
    }
}
