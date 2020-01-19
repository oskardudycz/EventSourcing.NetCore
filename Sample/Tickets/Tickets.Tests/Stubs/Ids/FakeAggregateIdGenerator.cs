using System;
using Core.Aggregates;
using Core.Ids;

namespace Tickets.Tests.Stubs.Ids
{
    public class FakeAggregateIdGenerator<T> : IAggregateIdGenerator<T> where T : IAggregate
    {
        public Guid? LastGeneratedId { get; private set; }
        public Guid New() => (LastGeneratedId = Guid.NewGuid()).Value;
    }
}
