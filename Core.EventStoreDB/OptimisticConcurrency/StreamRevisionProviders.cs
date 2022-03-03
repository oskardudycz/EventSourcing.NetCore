namespace Core.EventStoreDB.OptimisticConcurrency;

public class EventStoreDBOptimisticConcurrencyScope
{
    private readonly EventStoreDBExpectedStreamRevisionProvider expectedStreamVersionProvider;
    private readonly EventStoreDBNextStreamRevisionProvider nextStreamVersionProvider;

    public EventStoreDBOptimisticConcurrencyScope(
        EventStoreDBExpectedStreamRevisionProvider expectedStreamVersionProvider,
        EventStoreDBNextStreamRevisionProvider nextStreamVersionProvider
    )
    {
        this.expectedStreamVersionProvider = expectedStreamVersionProvider;
        this.nextStreamVersionProvider = nextStreamVersionProvider;
    }

    public async Task Do(Func<ulong?, Task<ulong>> handler)
    {
        var expectedVersion = expectedStreamVersionProvider.Value;

        var nextVersion = await handler(expectedVersion);

        nextStreamVersionProvider.Set(nextVersion);
    }
}

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
