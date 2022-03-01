namespace Core.EventStoreDB.OptimisticConcurrency;

public class EventStoreDBExpectedStreamRevisionProvider
{
    public ulong? Value { get; private set; }

    public bool TrySet(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !ulong.TryParse(value, out var expectedRevision))
            return false;

        Value = expectedRevision;
        return true;
    }
}

public class EventStoreDBNextStreamRevisionProvider
{
    public ulong? Value { get; private set; }

    public void Set(ulong nextRevision)
    {
        Value = nextRevision;
    }
}
