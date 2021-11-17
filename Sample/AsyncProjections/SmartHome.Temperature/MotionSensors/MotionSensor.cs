using System;
using Core.Aggregates;
using SmartHome.Temperature.MotionSensors.InstallingMotionSensor;

namespace SmartHome.Temperature.MotionSensors;

public class MotionSensor: Aggregate
{
    public DateTime InstalledAt { get; private set; }

    // For serialization
    public MotionSensor() { }


    public static MotionSensor Install(Guid motionSensorId)
    {
        return new MotionSensor(motionSensorId, DateTime.UtcNow);
    }

    private MotionSensor(Guid motionSensorId, in DateTime installedAt)
    {
        var @event = MotionSensorInstalled.Create(motionSensorId, installedAt);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(MotionSensorInstalled @event)
    {
        Id = @event.MotionSensorId;
        InstalledAt = @event.InstalledAt;
    }
}