namespace Core.Events;

public record EventMetadata(
    string EventId,
    ulong StreamPosition,
    ulong LogPosition,
    PropagationContext? PropagationContext
);

public interface IEventEnvelope
{
    object Data { get; }
    EventMetadata Metadata { get; init; }
}

public record EventEnvelope<T>(
    T Data,
    EventMetadata Metadata
): IEventEnvelope where T : notnull
{
    object IEventEnvelope.Data => Data;
}

public static class EventEnvelope
{
    public static IEventEnvelope From(object data, EventMetadata metadata)
    {
        //TODO: Get rid of reflection!
        var type = typeof(EventEnvelope<>).MakeGenericType(data.GetType());
        return (IEventEnvelope)Activator.CreateInstance(type, data, metadata)!;
    }

    public static EventEnvelope<T> From<T>(T data) where T : notnull =>
        new(data, new EventMetadata(Guid.NewGuid().ToString(), 0, 0, null));
}
