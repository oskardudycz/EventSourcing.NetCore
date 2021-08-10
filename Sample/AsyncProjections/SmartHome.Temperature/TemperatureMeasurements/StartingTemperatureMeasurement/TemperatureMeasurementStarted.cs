using System;
using Core.Events;
using Newtonsoft.Json;

namespace SmartHome.Temperature.TemperatureMeasurements.StartingTemperatureMeasurement
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
            if (measurementId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(measurementId));

            return new TemperatureMeasurementStarted(measurementId, DateTimeOffset.UtcNow);
        }
    }
}
