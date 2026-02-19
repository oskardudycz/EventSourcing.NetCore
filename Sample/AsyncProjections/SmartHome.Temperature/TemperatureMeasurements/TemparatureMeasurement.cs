using Core.Aggregates;
using SmartHome.Temperature.TemperatureMeasurements.RecordingTemperature;
using SmartHome.Temperature.TemperatureMeasurements.StartingTemperatureMeasurement;

namespace SmartHome.Temperature.TemperatureMeasurements;

public class TemperatureMeasurement: Aggregate
{
    public DateTimeOffset Started { get; set; }
    public DateTimeOffset? LastRecorded { get; set; }

    public List<decimal> Mesurements { get; set; } = null!;

    // For serialization
    public TemperatureMeasurement() { }


    public static TemperatureMeasurement Start(
        Guid measurementId) =>
        new(
            measurementId
        );

    private TemperatureMeasurement(Guid measurementId)
    {
        if (measurementId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(measurementId));

        var @event = TemperatureMeasurementStarted.Create(
            measurementId
        );

        Enqueue(@event);
        Apply(@event);
    }

    public void Record(decimal temperature)
    {
        if (temperature < -273)
            throw new ArgumentOutOfRangeException(nameof(temperature));

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
        Mesurements = [];
    }


    public void Apply(TemperatureRecorded @event)
    {
        Mesurements.Add(@event.Temperature);
        LastRecorded = @event.MeasuredAt;
    }
}
