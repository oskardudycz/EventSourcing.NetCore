using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Aggregates;
using Core.Exceptions;
using MediatR;

namespace Core.Repositories;

public static class RepositoryExtensions
{
    public static async Task<T> Get<T>(this IRepository<T> repository, Guid id, CancellationToken cancellationToken = default) where T : IAggregate
    {
        var entity = await repository.Find(id, cancellationToken);

        return entity ?? throw AggregateNotFoundException.For<T>(id);
    }

    public static async Task<Unit> GetAndUpdate<T>(this IRepository<T> repository, Guid id, Action<T> action, CancellationToken cancellationToken = default) where T : IAggregate
    {
        var entity = await repository.Get<T>(id, cancellationToken);

        action(entity);

        await repository.Update(entity, cancellationToken);

        return Unit.Value;
    }
}