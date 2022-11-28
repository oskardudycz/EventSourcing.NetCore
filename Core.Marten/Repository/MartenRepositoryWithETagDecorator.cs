using Core.Aggregates;
using Core.Marten.OptimisticConcurrency;

namespace Core.Marten.Repository;

public class MartenRepositoryWithETagDecorator<T>: IMartenRepository<T> where T : class, IAggregate
{
    private readonly IMartenRepository<T> inner;
    private readonly MartenExpectedStreamVersionProvider expectedStreamVersionProvider;
    private readonly MartenNextStreamVersionProvider nextStreamVersionProvider;

    public MartenRepositoryWithETagDecorator(
        IMartenRepository<T> inner,
        MartenExpectedStreamVersionProvider expectedStreamVersionProvider,
        MartenNextStreamVersionProvider nextStreamVersionProvider
    )
    {
        this.inner = inner;
        this.expectedStreamVersionProvider = expectedStreamVersionProvider;
        this.nextStreamVersionProvider = nextStreamVersionProvider;
    }

    public Task<T?> Find(Guid id, CancellationToken ct) =>
        inner.Find(id, ct);

    public Task<long> Add(T aggregate, CancellationToken ct = default) =>
        SetNextExpectedVersion(inner.Add(aggregate, ct));

    public Task<long> Update(T aggregate, long? expectedVersion = null, CancellationToken ct = default) =>
        inner.Update(aggregate, expectedVersion ?? expectedStreamVersionProvider.Value, ct);

    public Task<long> Delete(T aggregate, long? expectedVersion = null, CancellationToken ct = default) =>
        inner.Delete(aggregate, expectedVersion ?? expectedStreamVersionProvider.Value, ct);

    private async Task<long> SetNextExpectedVersion(Task<long> action)
    {
        var nextExpectedVersion = await action;

        nextStreamVersionProvider.Set(nextExpectedVersion);

        return nextExpectedVersion;
    }
}
