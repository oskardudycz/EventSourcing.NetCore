using Core.Aggregates;
using Core.Marten.Repository;
using Core.Tracing;

namespace Tickets.Tests.Stubs.Storage;

public class FakeRepository<T> : IMartenRepository<T> where T : class, IAggregate
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

    public async Task<long> Add(T aggregate, TraceMetadata? traceMetadata = null, CancellationToken cancellationToken = default)
    {
        Aggregates.Add(aggregate.Id, aggregate);
        return await Task.FromResult(aggregate.Version);
    }

    public async Task<long> Update(T aggregate, long? expectedVersion = null, TraceMetadata? traceMetadata = null, CancellationToken cancellationToken = default)
    {
        Aggregates[aggregate.Id] = aggregate;
        return await Task.FromResult(aggregate.Version);
    }

    public async Task<long> Delete(T aggregate, long? expectedVersion = null, TraceMetadata? traceMetadata = null, CancellationToken cancellationToken = default)
    {
        Aggregates.Remove(aggregate.Id);
        return await Task.FromResult(aggregate.Version);
    }
}
