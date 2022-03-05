using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;

namespace SmartHome.Temperature.TemperatureMeasurements.RecordingTemperature;

public record RecordTemperature(
    Guid MeasurementId,
    decimal Temperature
): ICommand
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

public class HandleRecordTemperature:
    ICommandHandler<RecordTemperature>
{
    private readonly IMartenRepository<TemperatureMeasurement> repository;
    private readonly IMartenAppendScope scope;

    public HandleRecordTemperature(
        IMartenRepository<TemperatureMeasurement> repository,
        IMartenAppendScope scope
    )
    {
        this.repository = repository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(RecordTemperature command, CancellationToken cancellationToken)
    {
        var (measurementId, temperature) = command;

        await scope.Do((expectedVersion, traceMetadata) =>
            repository.GetAndUpdate(
                measurementId,
                reservation => reservation.Record(temperature),
                expectedVersion,
                traceMetadata,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
