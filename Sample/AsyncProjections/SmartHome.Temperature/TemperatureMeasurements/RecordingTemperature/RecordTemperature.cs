using Core.Commands;
using Core.Marten.Repository;

namespace SmartHome.Temperature.TemperatureMeasurements.RecordingTemperature;

public record RecordTemperature(
    Guid MeasurementId,
    decimal Temperature
)
{
    public static RecordTemperature Create(Guid measurementId, decimal temperature)
    {
        if (measurementId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(measurementId));
        if (temperature < -273)
            throw new ArgumentOutOfRangeException(nameof(temperature));

        return new RecordTemperature(measurementId, temperature);
    }
}

public class HandleRecordTemperature(IMartenRepository<TemperatureMeasurement> repository):
    ICommandHandler<RecordTemperature>
{
    public Task Handle(RecordTemperature command, CancellationToken ct)
    {
        var (measurementId, temperature) = command;

        return repository.GetAndUpdate(
            measurementId,
            reservation => reservation.Record(temperature),
            ct: ct
        );
    }
}
