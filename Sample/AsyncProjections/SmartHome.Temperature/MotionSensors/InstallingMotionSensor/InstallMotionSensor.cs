using Core.Commands;
using Core.Marten.Repository;

namespace SmartHome.Temperature.MotionSensors.InstallingMotionSensor;

public record InstallMotionSensor(
    Guid MotionSensorId
)
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

public class HandleInstallMotionSensor(IMartenRepository<MotionSensor> repository):
    ICommandHandler<InstallMotionSensor>
{
    public Task Handle(InstallMotionSensor command, CancellationToken ct) =>
        repository.Add(
            command.MotionSensorId,
            MotionSensor.Install(
                command.MotionSensorId
            ),
            ct
        );
}
