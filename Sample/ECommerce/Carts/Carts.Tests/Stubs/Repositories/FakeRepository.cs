using Core.Aggregates;
using Core.Marten.Repository;

namespace Carts.Tests.Stubs.Repositories;

public class FakeRepository<T>: IMartenRepository<T> where T : class, IAggregate
{
    public Dictionary<Guid, T> Aggregates { get; }

    public FakeRepository(params (Guid,T)[] aggregates) =>
        Aggregates = aggregates.ToDictionary(ks => ks.Item1, vs => vs.Item2);

    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Aggregates.GetValueOrDefault(id));

    public async Task<long> Add(Guid id, T aggregate, CancellationToken cancellationToken = default)
    {
        Aggregates.Add(id, aggregate);
        return await Task.FromResult(aggregate.Version);
    }

    public async Task<long> Update(Guid id, T aggregate, long? expectedVersion = null,
        CancellationToken cancellationToken = default)
    {
        Aggregates[id] = aggregate;
        return await Task.FromResult(aggregate.Version);
    }

    public async Task<long> Delete(Guid id, T aggregate, long? expectedVersion = null,
        CancellationToken cancellationToken = default)
    {
        Aggregates.Remove(id);
        return await Task.FromResult(aggregate.Version);
    }
}
