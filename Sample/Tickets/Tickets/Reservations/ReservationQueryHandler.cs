using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Queries;
using Marten;
using Marten.Pagination;
using MediatR;
using Tickets.Reservations.Projections;
using Tickets.Reservations.Queries;

namespace Tickets.Reservations
{
    internal class ReservationQueryHandler :
        IQueryHandler<GetReservationById, ReservationDetails>,
        IRequestHandler<GetReservations, IPagedList<ReservationShortInfo>>,
        IRequestHandler<GetReservationHistory, IPagedList<ReservationHistory>>,
        IRequestHandler<GetReservationAtVersion, ReservationDetails>
    {
        private readonly IDocumentSession querySession;

        public ReservationQueryHandler(IDocumentSession querySession)
        {
            Guard.Against.Null(querySession, nameof(querySession));

            this.querySession = querySession;
        }

        public Task<ReservationDetails> Handle(GetReservationById request, CancellationToken cancellationToken)
        {
            return querySession.LoadAsync<ReservationDetails>(request.ReservationId, cancellationToken);
        }

        public Task<IPagedList<ReservationShortInfo>> Handle(GetReservations request, CancellationToken cancellationToken)
        {
            return querySession.Query<ReservationShortInfo>()
                .ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }

        public Task<IPagedList<ReservationHistory>> Handle(GetReservationHistory request, CancellationToken cancellationToken)
        {
            return querySession.Query<ReservationHistory>()
                .Where(h => h.ReservationId == request.ReservationId)
                .ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }

        public Task<ReservationDetails> Handle(GetReservationAtVersion request, CancellationToken cancellationToken)
        {
            return querySession.Events.AggregateStreamAsync<ReservationDetails>(request.ReservationId, request.Version, token: cancellationToken);
        }
    }
}
