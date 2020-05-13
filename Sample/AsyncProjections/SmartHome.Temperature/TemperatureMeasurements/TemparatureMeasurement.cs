using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using Core.Aggregates;
using Marten.Events;
using SmartHome.Temperature.TemperatureMeasurements.Events;

namespace SmartHome.Temperature.TemperatureMeasurements
{
    public class TemperatureMeasurement: Aggregate
    {
        public DateTimeOffset Started { get; set; }
        public DateTimeOffset? LastRecorded { get; set; }

        public List<decimal> Mesurements { get; set; }

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
            Apply(new Event<TemperatureMeasurementStarted>(@event));
        }

        public void Record(decimal temperature)
        {
            Guard.Against.OutOfRange(temperature, nameof(temperature), -273, Decimal.MaxValue);

            var @event = TemperatureRecorded.Create(
                Id,
                temperature
            );

            Enqueue(@event);
            Apply(new Event<TemperatureRecorded>(@event));
        }

        public void Apply(Event<TemperatureMeasurementStarted> @event)
        {
            Id = @event.Data.MeasurementId;
            Started = @event.Timestamp;
            Mesurements = new List<decimal>();
        }


        public void Apply(Event<TemperatureRecorded> @event)
        {
            Mesurements.Add(@event.Data.Temperature);
            LastRecorded = @event.Timestamp;
        }
    }
}
