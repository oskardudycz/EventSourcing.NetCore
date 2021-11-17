using System;
using Core.Events;
using Newtonsoft.Json;

namespace SmartHome.Temperature.TemperatureMeasurements.RecordingTemperature;

public class TemperatureRecorded : IEvent
{
    public Guid MeasurementId { get; }

    public decimal Temperature { get; }

    public DateTimeOffset MeasuredAt { get; }

    [JsonConstructor]
    private TemperatureRecorded(Guid measurementId, decimal temperature, DateTimeOffset measuredAt)
    {
        MeasurementId = measurementId;
        Temperature = temperature;
        MeasuredAt = measuredAt;
    }

    public static TemperatureRecorded Create(Guid measurementId, decimal temperature)
    {
        if (measurementId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(measurementId));
        if (temperature < -273)
            throw new ArgumentOutOfRangeException(nameof(temperature));

        return new TemperatureRecorded(measurementId, temperature, DateTimeOffset.UtcNow);
    }
}