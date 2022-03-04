using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;

namespace SmartHome.Temperature.TemperatureMeasurements.StartingTemperatureMeasurement;

public record StartTemperatureMeasurement(
    Guid MeasurementId
): ICommand
{
    public static StartTemperatureMeasurement Create(Guid measurementId)
    {
        if (measurementId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(measurementId));

        return new StartTemperatureMeasurement(measurementId);
    }
}


public class HandleStartTemperatureMeasurement:
    ICommandHandler<StartTemperatureMeasurement>
{
    private readonly IMartenRepository<TemperatureMeasurement> repository;
    private readonly IMartenAppendScope scope;

    public HandleStartTemperatureMeasurement(
        IMartenRepository<TemperatureMeasurement> repository,
        IMartenAppendScope scope
    )
    {
        this.repository = repository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(StartTemperatureMeasurement command, CancellationToken cancellationToken)
    {
        await scope.Do((_, eventMetadata) =>
            repository.Add(
                TemperatureMeasurement.Start(
                    command.MeasurementId
                ),
                eventMetadata,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
