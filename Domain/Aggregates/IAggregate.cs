using System;

namespace Domain.Aggregates
{
    public interface IAggregate
    {
        Guid Id { get; }
    }
}
