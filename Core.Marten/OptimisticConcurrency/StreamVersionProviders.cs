namespace Core.Marten.OptimisticConcurrency;

public class MartenOptimisticConcurrencyScope
{
    private readonly MartenExpectedStreamVersionProvider expectedStreamVersionProvider;
    private readonly MartenNextStreamVersionProvider nextStreamVersionProvider;

    public MartenOptimisticConcurrencyScope(
        MartenExpectedStreamVersionProvider expectedStreamVersionProvider,
        MartenNextStreamVersionProvider nextStreamVersionProvider
    )
    {
        this.expectedStreamVersionProvider = expectedStreamVersionProvider;
        this.nextStreamVersionProvider = nextStreamVersionProvider;
    }

    public async Task Do(Func<long?, Task<long>> handler)
    {
        var expectedVersion = expectedStreamVersionProvider.Value;

        var nextVersion = await handler(expectedVersion);

        nextStreamVersionProvider.Set(nextVersion);
    }
}

public class MartenExpectedStreamVersionProvider
{
    public long? Value { get; private set; }

    public bool TrySet(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !long.TryParse(value, out var expectedVersion))
            return false;

        Value = expectedVersion;
        return true;
    }
}

public class MartenNextStreamVersionProvider
{
    public long? Value { get; private set; }

    public void Set(long nextVersion)
    {
        Value = nextVersion;
    }
}
