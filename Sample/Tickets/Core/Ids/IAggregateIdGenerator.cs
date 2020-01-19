using System;
using Core.Aggregates;

namespace Core.Ids
{
    public interface IAggregateIdGenerator<T> where T : IAggregate
    {
        Guid New();
    }
}
