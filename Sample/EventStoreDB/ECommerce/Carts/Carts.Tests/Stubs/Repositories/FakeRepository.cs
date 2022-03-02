using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Aggregates;
using Core.EventStoreDB.Repository;

namespace Carts.Tests.Stubs.Repositories;

public class FakeRepository<T> : IEventStoreDBRepository<T> where T : class, IAggregate
{
    public Dictionary<Guid, T> Aggregates { get; private set; }

    public FakeRepository(params T[] aggregates)
    {
        Aggregates = aggregates.ToDictionary(ks=> ks.Id, vs => vs);
    }

    public Task<T?> Find(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(Aggregates.GetValueOrDefault(id));
    }

    public async Task<ulong> Add(T aggregate, CancellationToken cancellationToken = default)
    {
        Aggregates.Add(aggregate.Id, aggregate);
        return await Task.FromResult((ulong)aggregate.Version);
    }

    public async Task<ulong> Update(T aggregate, ulong? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        Aggregates[aggregate.Id] = aggregate;
        return await Task.FromResult((ulong)aggregate.Version);
    }

    public async Task<ulong> Delete(T aggregate, ulong? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        Aggregates.Remove(aggregate.Id);
        return await Task.FromResult((ulong)aggregate.Version);
    }
}
