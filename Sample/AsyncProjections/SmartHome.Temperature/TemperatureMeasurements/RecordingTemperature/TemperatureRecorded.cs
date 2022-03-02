using System;
using Core.Events;

namespace SmartHome.Temperature.TemperatureMeasurements.RecordingTemperature;

public record TemperatureRecorded(
    Guid MeasurementId,
    decimal Temperature,
    DateTimeOffset MeasuredAt
): IEvent
{
    public static TemperatureRecorded Create(Guid measurementId, decimal temperature)
    {
        if (measurementId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(measurementId));
        if (temperature < -273)
            throw new ArgumentOutOfRangeException(nameof(temperature));

        return new TemperatureRecorded(measurementId, temperature, DateTimeOffset.UtcNow);
    }
}
