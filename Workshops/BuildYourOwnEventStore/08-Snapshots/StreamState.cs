namespace EventStoreBasics;

public class StreamState(Guid id, Type type, long version)
{
    public Guid Id { get; } = id;

    public Type Type { get; } = type;

    public long Version { get; } = version;
}
