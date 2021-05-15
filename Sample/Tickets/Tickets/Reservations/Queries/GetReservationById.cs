using System;
using Core.Queries;
using Tickets.Reservations.Projections;

namespace Tickets.Reservations.Queries
{
    public record GetReservationById(
        Guid ReservationId
    ): IQuery<ReservationDetails>;
}
