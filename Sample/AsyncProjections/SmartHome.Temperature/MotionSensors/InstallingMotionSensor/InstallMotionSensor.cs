using Core.Commands;
using Core.Marten.Events;
using Core.Marten.Repository;
using MediatR;

namespace SmartHome.Temperature.MotionSensors.InstallingMotionSensor;

public record InstallMotionSensor(
    Guid MotionSensorId
) : ICommand
{
    public static InstallMotionSensor Create(
        Guid motionSensorId
    )
    {
        if (motionSensorId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(motionSensorId));

        return new InstallMotionSensor(motionSensorId);
    }
}

public class HandleInstallMotionSensor :
    ICommandHandler<InstallMotionSensor>
{
    private readonly IMartenRepository<MotionSensor> repository;
    private readonly IMartenAppendScope scope;

    public HandleInstallMotionSensor(
        IMartenRepository<MotionSensor> repository,
        IMartenAppendScope scope
    )
    {
        this.repository = repository;
        this.scope = scope;
    }

    public async Task<Unit> Handle(InstallMotionSensor command, CancellationToken cancellationToken)
    {
        await scope.Do((_, eventMetadata) =>
            repository.Add(
                MotionSensor.Install(
                    command.MotionSensorId
                ),
                eventMetadata,
                cancellationToken
            )
        );
        return Unit.Value;
    }
}
