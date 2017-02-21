using Domain.Events;
using System;
using System.Collections.Generic;

namespace Domain.Aggregates
{
    public interface IAggregate
    {
        Guid Id { get; }
    }
}
