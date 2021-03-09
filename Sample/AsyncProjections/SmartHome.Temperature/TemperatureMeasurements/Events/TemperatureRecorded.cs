using System;
using Ardalis.GuardClauses;
using Core.Events;
using Newtonsoft.Json;

namespace SmartHome.Temperature.TemperatureMeasurements.Events
{
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
            Guard.Against.Default(measurementId, nameof(measurementId));
            Guard.Against.OutOfRange(temperature, nameof(temperature), -273, decimal.MaxValue);

            return new TemperatureRecorded(measurementId, temperature, DateTimeOffset.UtcNow);
        }
    }
}
