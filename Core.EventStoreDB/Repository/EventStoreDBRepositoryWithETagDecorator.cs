using Core.Aggregates;
using Core.EventStoreDB.OptimisticConcurrency;
using Microsoft.Extensions.DependencyInjection;

namespace Core.EventStoreDB.Repository;

public class EventStoreDBRepositoryWithETagDecorator<T>: IEventStoreDBRepository<T>
    where T : class, IAggregate
{
    private readonly IEventStoreDBRepository<T> inner;
    private readonly Func<ulong?> getExpectedVersion;
    private readonly Action<ulong> setNextExpectedVersion;

    public EventStoreDBRepositoryWithETagDecorator(
        IEventStoreDBRepository<T> inner,
        Func<ulong?> getExpectedVersion,
        Action<ulong> setNextExpectedVersion
    )
    {
        this.inner = inner;
        this.getExpectedVersion = getExpectedVersion;
        this.setNextExpectedVersion = setNextExpectedVersion;
    }

    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        inner.Find(id, cancellationToken);

    public async Task<ulong> Add(T aggregate, CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Add(
            aggregate,
            cancellationToken
        ).ConfigureAwait(true);

        setNextExpectedVersion(nextExpectedVersion);

        return nextExpectedVersion;
    }

    public async Task<ulong> Update(T aggregate, ulong? expectedVersion = null,
        CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Update(
            aggregate,
            expectedVersion ?? getExpectedVersion(),
            cancellationToken
        ).ConfigureAwait(true);

        setNextExpectedVersion(nextExpectedVersion);

        return nextExpectedVersion;
    }

    public async Task<ulong> Delete(T aggregate, ulong? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Delete(
            aggregate,
            expectedVersion ?? getExpectedVersion(),
            cancellationToken
        ).ConfigureAwait(true);

        setNextExpectedVersion(nextExpectedVersion);

        return nextExpectedVersion;
    }
}

public static class EventStoreDBAppendScopeExtensions
{
    public static IServiceCollection AddEventStoreDBAppendScope(this IServiceCollection services) =>
        services
            .AddScoped<EventStoreDBExpectedStreamRevisionProvider, EventStoreDBExpectedStreamRevisionProvider>()
            .AddScoped<EventStoreDBNextStreamRevisionProvider, EventStoreDBNextStreamRevisionProvider>();
}
