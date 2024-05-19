using Core.Queries;
using Marten;

namespace SmartHome.Temperature.TemperatureMeasurements.GettingTemperatureMeasurements;

public record GetTemperatureMeasurements;

public class HandleGetTemperatureMeasurements(IDocumentSession querySession)
    : IQueryHandler<GetTemperatureMeasurements, IReadOnlyList<TemperatureMeasurement>>
{
    public Task<IReadOnlyList<TemperatureMeasurement>> Handle(GetTemperatureMeasurements request, CancellationToken cancellationToken) =>
        querySession.Query<TemperatureMeasurement>().ToListAsync(cancellationToken);
}
