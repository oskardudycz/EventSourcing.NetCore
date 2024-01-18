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

public class HandleInstallMotionSensor:
    ICommandHandler<InstallMotionSensor>
{
    private readonly IMartenRepository<MotionSensor> repository;

    public HandleInstallMotionSensor(IMartenRepository<MotionSensor> repository) =>
        this.repository = repository;

    public Task Handle(InstallMotionSensor command, CancellationToken ct) =>
        repository.Add(
            MotionSensor.Install(
                command.MotionSensorId
            ),
            ct
        );
}
