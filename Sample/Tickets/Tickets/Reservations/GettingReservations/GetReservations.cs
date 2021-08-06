using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Queries;
using Marten;
using Marten.Pagination;
using MediatR;

namespace Tickets.Reservations.GettingReservations
{
    public class GetReservations: IQuery<IPagedList<ReservationShortInfo>>
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

    internal class HandleGetReservations:
        IQueryHandler<GetReservations, IPagedList<ReservationShortInfo>>
    {
        private readonly IDocumentSession querySession;

        public HandleGetReservations(IDocumentSession querySession)
        {
            this.querySession = querySession;
        }

        public Task<IPagedList<ReservationShortInfo>> Handle(GetReservations request,
            CancellationToken cancellationToken)
        {
            return querySession.Query<ReservationShortInfo>()
                .ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
