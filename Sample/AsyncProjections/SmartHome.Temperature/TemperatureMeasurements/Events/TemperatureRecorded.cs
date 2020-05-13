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

        [JsonConstructor]
        private TemperatureRecorded(Guid measurementId, decimal temperature)
        {
            MeasurementId = measurementId;
            Temperature = temperature;
        }

        public static TemperatureRecorded Create(Guid measurementId, decimal temperature)
        {
            Guard.Against.Default(measurementId, nameof(measurementId));
            Guard.Against.OutOfRange(temperature, nameof(temperature), -273, Decimal.MaxValue);

            return new TemperatureRecorded(measurementId, temperature);
        }
    }
}
