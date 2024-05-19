using Core.Aggregates;
using Core.EventStoreDB.Repository;

namespace Carts.Tests.Stubs.Repositories;

public class FakeRepository<T> : IEventStoreDBRepository<T> where T : class, IAggregate
{
    public Dictionary<Guid, T> Aggregates { get; }

    public FakeRepository(params (Guid, T)[] aggregates) =>
        Aggregates = aggregates.ToDictionary(ks=> ks.Item1, vs => vs.Item2);

    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Aggregates.GetValueOrDefault(id));

    public async Task<ulong> Add(Guid id, T aggregate, CancellationToken cancellationToken = default)
    {
        Aggregates.Add(id, aggregate);
        return await Task.FromResult((ulong)aggregate.Version);
    }

    public async Task<ulong> Update(Guid id, T aggregate, ulong? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        Aggregates[id] = aggregate;
        return await Task.FromResult((ulong)aggregate.Version);
    }

    public async Task<ulong> Delete(Guid id, T aggregate, ulong? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        Aggregates.Remove(id);
        return await Task.FromResult((ulong)aggregate.Version);
    }
}
