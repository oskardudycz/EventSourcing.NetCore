using Core.Aggregates;
using Core.Exceptions;
using Core.Tracing;

namespace Core.Marten.Repository;

public static class RepositoryExtensions
{
    public static async Task<T> Get<T>(
        this IMartenRepository<T> repository,
        Guid id,
        CancellationToken cancellationToken = default
    ) where T : class, IAggregate
    {
        var entity = await repository.Find(id, cancellationToken);

        return entity ?? throw AggregateNotFoundException.For<T>(id);
    }

    public static async Task<long> GetAndUpdate<T>(
        this IMartenRepository<T> repository,
        Guid id,
        Action<T> action,
        long? expectedVersion = null,
        TraceMetadata? traceMetadata = null,
        CancellationToken cancellationToken = default
    ) where T : class, IAggregate
    {
        var entity = await repository.Get(id, cancellationToken);

        action(entity);

        return await repository.Update(entity, expectedVersion, traceMetadata, cancellationToken);
    }
}
