namespace EventStoreBasics;

public class Repository<T>(IEventStore eventStore): IRepository<T>
    where T : IAggregate
{
    private readonly IEventStore eventStore = eventStore ?? throw  new ArgumentNullException(nameof(eventStore));

    public Task<T?> Find(Guid id, CancellationToken ct = default) =>
        eventStore.AggregateStream<T>(id, ct: ct);

    public Task Add(T aggregate, CancellationToken ct = default) =>
        eventStore.Store(aggregate, ct);

    public Task Update(T aggregate, CancellationToken ct = default) =>
        eventStore.Store(aggregate, ct);

    public Task Delete(T aggregate, CancellationToken ct = default) =>
        eventStore.Store(aggregate, ct);
}
