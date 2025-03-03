namespace HotelManagement.EventStorage;

public class StreamTransformations
{
    private readonly List<Func<List<SerializedEvent>, List<SerializedEvent>>> jsonTransformations = [];

    public List<SerializedEvent> Transform(List<SerializedEvent> events)
    {
        if (!jsonTransformations.Any())
            return events;

        var result = jsonTransformations
            .Aggregate(events, (current, transform) => transform(current));

        return result;
    }

    public StreamTransformations Register(
        Func<List<SerializedEvent>, List<SerializedEvent>> transformJson
    )
    {
        jsonTransformations.Add(transformJson);
        return this;
    }
}
