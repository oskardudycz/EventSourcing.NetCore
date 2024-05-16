using Core.Commands;
using Core.Marten.Repository;

namespace SmartHome.Temperature.TemperatureMeasurements.StartingTemperatureMeasurement;

public record StartTemperatureMeasurement(
    Guid MeasurementId
)
{
    public static StartTemperatureMeasurement Create(Guid measurementId)
    {
        if (measurementId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(measurementId));

        return new StartTemperatureMeasurement(measurementId);
    }
}

public class HandleStartTemperatureMeasurement(IMartenRepository<TemperatureMeasurement> repository):
    ICommandHandler<StartTemperatureMeasurement>
{
    public Task Handle(StartTemperatureMeasurement command, CancellationToken ct) =>
        repository.Add(
            command.MeasurementId,
            TemperatureMeasurement.Start(
                command.MeasurementId
            ),
            ct
        );
}
