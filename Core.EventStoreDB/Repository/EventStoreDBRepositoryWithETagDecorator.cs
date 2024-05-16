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

    public async Task<ulong> Add(Guid id, T aggregate, CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Add(
            id,
            aggregate,
            cancellationToken
        ).ConfigureAwait(true);

        nextResourceVersionProvider.TrySet(nextExpectedVersion.ToString());

        return nextExpectedVersion;
    }

    public async Task<ulong> Update(Guid id, T aggregate, ulong? expectedVersion = null,
        CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Update(
            id,
            aggregate,
            expectedVersion ?? GetExpectedVersion(),
            cancellationToken
        ).ConfigureAwait(true);

        nextResourceVersionProvider.TrySet(nextExpectedVersion.ToString());

        return nextExpectedVersion;
    }

    public async Task<ulong> Delete(Guid id, T aggregate, ulong? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Delete(
            id,
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
