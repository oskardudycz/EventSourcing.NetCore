using System;
using Ardalis.GuardClauses;
using Core.Events;
using Newtonsoft.Json;

namespace SmartHome.Temperature.TemperatureMeasurements.Events
{
    public class TemperatureMeasurementStarted : IEvent
    {
        public Guid MeasurementId { get; }

        [JsonConstructor]
        private TemperatureMeasurementStarted(Guid measurementId)
        {
            MeasurementId = measurementId;
        }

        public static TemperatureMeasurementStarted Create(Guid measurementId)
        {
            Guard.Against.Default(measurementId, nameof(measurementId));

            return new TemperatureMeasurementStarted(measurementId);
        }
    }
}
