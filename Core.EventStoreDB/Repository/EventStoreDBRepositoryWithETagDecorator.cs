using Core.Aggregates;
using Core.OptimisticConcurrency;

namespace Core.EventStoreDB.Repository;

public class EventStoreDBRepositoryWithETagDecorator<T>(
    IEventStoreDBRepository<T> inner,
    IExpectedResourceVersionProvider expectedResourceVersionProvider,
    INextResourceVersionProvider nextResourceVersionProvider)
    : IEventStoreDBRepository<T>
    where T : class, IAggregate
{
    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        inner.Find(id, cancellationToken);

    public async Task<ulong> Add(T aggregate, CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Add(
            aggregate,
            cancellationToken
        ).ConfigureAwait(true);

        nextResourceVersionProvider.TrySet(nextExpectedVersion.ToString());

        return nextExpectedVersion;
    }

    public async Task<ulong> Update(T aggregate, ulong? expectedVersion = null,
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

    public async Task<ulong> Delete(T aggregate, ulong? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Delete(
            aggregate,
            expectedVersion ?? GetExpectedVersion(),
            cancellationToken
        ).ConfigureAwait(true);

        nextResourceVersionProvider.TrySet(nextExpectedVersion.ToString());

        return nextExpectedVersion;
    }

    private ulong? GetExpectedVersion()
    {
        var value = expectedResourceVersionProvider.Value;

        if (string.IsNullOrWhiteSpace(value) || !ulong.TryParse(value, out var expectedVersion))
            return null;

        return expectedVersion;
    }
}
