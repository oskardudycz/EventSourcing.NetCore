namespace HotelManagement.EventStore;

public record EventMetadata(
    Guid CorrelationId
);

public record EventData(
    string EventType,
    string Data,
    string MetaData
);

public class StreamTransformations
{
    private readonly List<Func<List<EventData>, List<EventData>>> jsonTransformations = [];

    public List<EventData> Transform(List<EventData> events)
    {
        if (!jsonTransformations.Any())
            return events;

        var result = jsonTransformations
            .Aggregate(events, (current, transform) => transform(current));

        return result;
    }

    public StreamTransformations Register(
        Func<List<EventData>, List<EventData>> transformJson
    )
    {
        jsonTransformations.Add(transformJson);
        return this;
    }
}
