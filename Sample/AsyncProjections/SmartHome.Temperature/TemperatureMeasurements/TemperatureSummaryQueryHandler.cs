using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Queries;
using Marten;
using Marten.Linq;
using Marten.Pagination;
using MediatR;
using SmartHome.Temperature.TemperatureMeasurements.Queries;

namespace SmartHome.Temperature.TemperatureMeasurements
{
    public class TemperatureSummaryQueryHandler: IQueryHandler<GetTemperatureMeasurements, IReadOnlyList<TemperatureMeasurement>>
    {
        private readonly IDocumentSession querySession;

        public TemperatureSummaryQueryHandler(IDocumentSession querySession)
        {
            Guard.Against.Null(querySession, nameof(querySession));

            this.querySession = querySession;
        }

        public Task<IReadOnlyList<TemperatureMeasurement>> Handle(GetTemperatureMeasurements request, CancellationToken cancellationToken)
        {
            return querySession.Query<TemperatureMeasurement>().ToListAsync(cancellationToken);
        }
    }
}
