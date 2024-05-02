using Core.Aggregates;
using Core.OptimisticConcurrency;

namespace Core.Marten.Repository;

public class MartenRepositoryWithETagDecorator<T>(
    IMartenRepository<T> inner,
    IExpectedResourceVersionProvider expectedResourceVersionProvider,
    INextResourceVersionProvider nextResourceVersionProvider)
    : IMartenRepository<T>
    where T : class, IAggregate
{
    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        inner.Find(id, cancellationToken);

    public async Task<long> Add(T aggregate, CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Add(
            aggregate,
            cancellationToken
        ).ConfigureAwait(true);

        nextResourceVersionProvider.TrySet(nextExpectedVersion.ToString());

        return nextExpectedVersion;
    }

    public async Task<long> Update(T aggregate, long? expectedVersion = null,
        CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Update(
            aggregate,
            expectedVersion ?? GetExpectedVersion(),
            cancellationToken
        ).ConfigureAwait(true);

        nextResourceVersionProvider.TrySet(nextExpectedVersion.ToString());

        return nextExpectedVersion;
    }

    public async Task<long> Delete(T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Delete(
            aggregate,
            expectedVersion ?? GetExpectedVersion(),
            cancellationToken
        ).ConfigureAwait(true);

        nextResourceVersionProvider.TrySet(nextExpectedVersion.ToString());

        return nextExpectedVersion;
    }

    private long? GetExpectedVersion()
    {
        var value = expectedResourceVersionProvider.Value;

        if (string.IsNullOrWhiteSpace(value) || !long.TryParse(value, out var expectedVersion))
            return null;

        return expectedVersion;
    }
}
