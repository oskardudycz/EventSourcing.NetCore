using System;
using Ardalis.GuardClauses;
using Core.Commands;

namespace SmartHome.Temperature.TemperatureMeasurements.Commands
{
    public class StartTemperatureMeasurement: ICommand
    {
        public Guid MeasurementId { get; }

        private StartTemperatureMeasurement(Guid measurementId)
        {
            MeasurementId = measurementId;
        }

        public static StartTemperatureMeasurement Create(Guid measurementId)
        {
            Guard.Against.Default(measurementId, nameof(measurementId));

            return new StartTemperatureMeasurement(measurementId);
        }
    }
}
