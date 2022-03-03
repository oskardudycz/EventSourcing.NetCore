using Core.Queries;
using Marten;

namespace SmartHome.Temperature.TemperatureMeasurements.GettingTemperatureMeasurements;

public record GetTemperatureMeasurements: IQuery<IReadOnlyList<TemperatureMeasurement>>;

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
