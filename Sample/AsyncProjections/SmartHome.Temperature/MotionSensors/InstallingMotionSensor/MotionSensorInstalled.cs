namespace SmartHome.Temperature.MotionSensors.InstallingMotionSensor;

public record MotionSensorInstalled(
    Guid MotionSensorId,
    DateTime InstalledAt
)
{
    public static MotionSensorInstalled Create(
        Guid motionSensorId,
        DateTime installedAt
    )
    {
        if (motionSensorId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(motionSensorId));
        if (installedAt == default)
            throw new ArgumentOutOfRangeException(nameof(installedAt));

        return new MotionSensorInstalled(motionSensorId, installedAt);
    }
}
