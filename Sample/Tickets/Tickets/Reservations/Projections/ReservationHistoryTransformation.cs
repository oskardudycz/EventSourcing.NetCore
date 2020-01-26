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

    public class ReservationHistoryTransformation : ITransform<TentativeReservationCreated, ReservationHistory>
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
    }
}
