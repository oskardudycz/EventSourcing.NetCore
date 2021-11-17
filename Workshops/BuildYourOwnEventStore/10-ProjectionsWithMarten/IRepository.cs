using System;

namespace EventStoreBasics;

public interface IRepository<T> where T : IAggregate
{
    T? Find(Guid id);

    void Add(T aggregate);

    void Update(T aggregate);

    void Delete(T aggregate);
}