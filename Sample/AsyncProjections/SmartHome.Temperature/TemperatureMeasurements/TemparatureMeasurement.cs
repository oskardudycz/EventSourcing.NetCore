using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using Core.Aggregates;
using SmartHome.Temperature.TemperatureMeasurements.Events;

namespace SmartHome.Temperature.TemperatureMeasurements
{
    public class TemperatureMeasurement: Aggregate
    {
        public DateTimeOffset Started { get; set; }
        public DateTimeOffset? LastRecorded { get; set; }

        public List<decimal> Mesurements { get; set; } = default!;

        // For serialization
        public TemperatureMeasurement() { }


        public static TemperatureMeasurement Start(
            Guid measurementId)
        {
            return new TemperatureMeasurement(
                measurementId
            );
        }

        private TemperatureMeasurement(Guid measurementId)
        {
            Guard.Against.Default(measurementId, nameof(measurementId));

            var @event = TemperatureMeasurementStarted.Create(
                measurementId
            );

            Enqueue(@event);
            Apply(@event);
        }

        public void Record(decimal temperature)
        {
            Guard.Against.OutOfRange(temperature, nameof(temperature), -273, Decimal.MaxValue);

            var @event = TemperatureRecorded.Create(
                Id,
                temperature
            );

            Enqueue(@event);
            Apply(@event);
        }

        public void Apply(TemperatureMeasurementStarted @event)
        {
            Id = @event.MeasurementId;
            Started = @event.StartedAt;
            Mesurements = new List<decimal>();
        }


        public void Apply(TemperatureRecorded @event)
        {
            Mesurements.Add(@event.Temperature);
            LastRecorded = @event.MeasuredAt;
        }
    }
}
