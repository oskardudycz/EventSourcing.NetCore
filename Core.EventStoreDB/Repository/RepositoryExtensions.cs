using Core.Aggregates;
using Core.Exceptions;
using Core.Tracing;

namespace Core.EventStoreDB.Repository;

public static class RepositoryExtensions
{
    public static async Task<T> Get<T>(
        this IEventStoreDBRepository<T> repository,
        Guid id,
        CancellationToken ct
    ) where T : class, IAggregate
    {
        var entity = await repository.Find(id, ct);

        return entity ?? throw AggregateNotFoundException.For<T>(id);
    }

    public static async Task<ulong> GetAndUpdate<T>(
        this IEventStoreDBRepository<T> repository,
        Guid id,
        Action<T> action,
        ulong? expectedVersion = null,
        TraceMetadata? traceMetadata = null,
        CancellationToken ct = default
    ) where T : class, IAggregate
    {
        var entity = await repository.Get(id, ct);

        action(entity);

        return await repository.Update(entity, expectedVersion, traceMetadata, ct);
    }
}
