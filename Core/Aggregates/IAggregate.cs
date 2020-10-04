using System;

namespace Core.Aggregates
{
    public interface IAggregate
    {
        Guid Id { get; }
    }
}
