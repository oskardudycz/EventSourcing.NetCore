using System;
using Ardalis.GuardClauses;
using Core.Events;
using Newtonsoft.Json;

namespace SmartHome.Temperature.TemperatureMeasurements.Events
{
    public class TemperatureMeasurementStarted : IEvent
    {
        public Guid MeasurementId { get; }
        public DateTimeOffset StartedAt { get; }

        [JsonConstructor]
        private TemperatureMeasurementStarted(Guid measurementId, DateTimeOffset startedAt)
        {
            MeasurementId = measurementId;
            StartedAt = startedAt;
        }

        public static TemperatureMeasurementStarted Create(Guid measurementId)
        {
            Guard.Against.Default(measurementId, nameof(measurementId));

            return new TemperatureMeasurementStarted(measurementId, DateTimeOffset.UtcNow);
        }
    }
}
