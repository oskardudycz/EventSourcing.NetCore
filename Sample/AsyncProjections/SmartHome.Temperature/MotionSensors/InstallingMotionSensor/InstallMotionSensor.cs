using Core.Commands;
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

    public HandleInstallMotionSensor(
        IMartenRepository<MotionSensor> repository
    )
    {
        this.repository = repository;
    }

    public async Task<Unit> Handle(InstallMotionSensor command, CancellationToken cancellationToken)
    {
        var reservation = MotionSensor.Install(
            command.MotionSensorId
        );

        await repository.Add(reservation, cancellationToken);

        return Unit.Value;
    }
}
