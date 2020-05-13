using System;
using Ardalis.GuardClauses;
using Core.Commands;

namespace SmartHome.Temperature.TemperatureMeasurements.Commands
{
    public class RecordTemperature : ICommand
    {
        public Guid MeasurementId { get; }

        public decimal Temperature { get; }

        private RecordTemperature(Guid measurementId, decimal temperature)
        {
            MeasurementId = measurementId;
            Temperature = temperature;
        }

        public static RecordTemperature Create(Guid measurementId, decimal temperature)
        {
            Guard.Against.Default(measurementId, nameof(measurementId));
            Guard.Against.OutOfRange(temperature, nameof(temperature), -273, Decimal.MaxValue);

            return new RecordTemperature(measurementId, temperature);
        }
    }
}
