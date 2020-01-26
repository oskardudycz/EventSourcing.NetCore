using System;
using Ardalis.GuardClauses;
using Core.Queries;
using Marten.Pagination;
using Tickets.Reservations.Projections;

namespace Tickets.Reservations.Queries
{
    public class GetReservationHistory : IQuery<IPagedList<ReservationHistory>>
    {
        public Guid ReservationId { get; }
        public int PageNumber { get; }
        public int PageSize { get; }

        private GetReservationHistory(Guid reservationId, int pageNumber, int pageSize)
        {
            ReservationId = reservationId;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static GetReservationHistory Create(Guid reservationId,int pageNumber = 1, int pageSize = 20)
        {
            Guard.Against.NegativeOrZero(pageNumber, nameof(pageNumber));
            Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

            return new GetReservationHistory(reservationId, pageNumber, pageSize);
        }
    }
}
