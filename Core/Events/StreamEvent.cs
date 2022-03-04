namespace Core.Events;

public record StreamEventMetadata(
    ulong StreamRevision,
    ulong LogPosition
);

public class StreamEvent: IEvent
{
    public object Data { get; }
    public StreamEventMetadata Metadata { get; }

    public StreamEvent(object data, StreamEventMetadata metadata)
    {
        Data = data;
        Metadata = metadata;
    }
}

public class StreamEvent<T>: StreamEvent where T: notnull
{
    public new T Data => (T)base.Data;

    public StreamEvent(T data, StreamEventMetadata metadata) : base(data, metadata)
    {
    }
}

