using System;
using Ardalis.GuardClauses;
using Core.Queries;
using Marten.Pagination;
using Tickets.Reservations.Projections;

namespace Tickets.Reservations.Queries
{
    public class GetReservations : IQuery<IPagedList<ReservationShortInfo>>
    {
        public int PageNumber { get; }
        public int PageSize { get; }

        private GetReservations(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static GetReservations Create(int pageNumber = 1, int pageSize = 20)
        {
            Guard.Against.NegativeOrZero(pageNumber, nameof(pageNumber));
            Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

            return new GetReservations(pageNumber, pageSize);
        }
    }
}
