namespace EventStoreBasics;

public interface IRepository<T> where T : IAggregate
{
    Task<T?> Find(Guid id, CancellationToken ct = default);

    Task Add(T aggregate, CancellationToken ct = default);

    Task Update(T aggregate, CancellationToken ct = default);

    Task Delete(T aggregate, CancellationToken ct = default);
}
