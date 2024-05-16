using Core.Aggregates;
using Core.Exceptions;

namespace Core.EventStoreDB.Repository;

public static class RepositoryExtensions
{
    public static async Task<T> Get<T>(
        this IEventStoreDBRepository<T> repository,
        Guid id,
        CancellationToken ct
    ) where T : class, IAggregate
    {
        var entity = await repository.Find(id, ct).ConfigureAwait(false);

        return entity ?? throw AggregateNotFoundException.For<T>(id);
    }

    public static async Task<ulong> GetAndUpdate<T>(
        this IEventStoreDBRepository<T> repository,
        Guid id,
        Action<T> action,
        ulong? expectedVersion = null,
        CancellationToken ct = default
    ) where T : class, IAggregate
    {
        var entity = await repository.Get(id, ct).ConfigureAwait(false);

        action(entity);

        return await repository.Update(id, entity, expectedVersion, ct).ConfigureAwait(false);
    }
}
