using Core.Aggregates;
using Core.Exceptions;

namespace Core.Marten.Repository;

public static class RepositoryExtensions
{
    public static async Task<T> Get<T>(
        this IMartenRepository<T> repository,
        Guid id,
        CancellationToken cancellationToken = default
    ) where T : class
    {
        var entity = await repository.Find(id, cancellationToken).ConfigureAwait(false);

        return entity ?? throw AggregateNotFoundException.For<T>(id);
    }

    public static async Task<long> GetAndUpdate<T>(
        this IMartenRepository<T> repository,
        Guid id,
        Action<T> action,
        long? expectedVersion = null,
        CancellationToken ct = default
    ) where T : class
    {
        var entity = await repository.Get(id, ct).ConfigureAwait(false);

        action(entity);

        return await repository.Update(id, entity, expectedVersion, ct).ConfigureAwait(false);
    }
}
