using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using Marten;

namespace SmartHome.Temperature.TemperatureMeasurements.GettingTemperatureMeasurements
{
    public class GetTemperatureMeasurements: IQuery<IReadOnlyList<TemperatureMeasurement>>
    {
        public static GetTemperatureMeasurements Create()
        {
            return new();
        }
    }

    public class HandleGetTemperatureMeasurements: IQueryHandler<GetTemperatureMeasurements, IReadOnlyList<TemperatureMeasurement>>
    {
        private readonly IDocumentSession querySession;

        public HandleGetTemperatureMeasurements(IDocumentSession querySession)
        {
            this.querySession = querySession;
        }

        public Task<IReadOnlyList<TemperatureMeasurement>> Handle(GetTemperatureMeasurements request, CancellationToken cancellationToken)
        {
            return querySession.Query<TemperatureMeasurement>().ToListAsync(cancellationToken);
        }
    }
}
