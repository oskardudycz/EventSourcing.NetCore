using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Aggregates;
using Core.Storage;

namespace Tickets.Tests.Stubs.Storage
{
    public class FakeRepository<T> : IRepository<T> where T : IAggregate
    {
        public Dictionary<Guid, T> Aggregates { get; private set; }

        public FakeRepository(params T[] aggregates)
        {
            Aggregates = aggregates.ToDictionary(ks=> ks.Id, vs => vs);
        }

        public Task<T> Find(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Aggregates.GetValueOrDefault(id));
        }

        public Task Add(T aggregate, CancellationToken cancellationToken)
        {
            Aggregates.Add(aggregate.Id, aggregate);
            return Task.CompletedTask;
        }

        public Task Update(T aggregate, CancellationToken cancellationToken)
        {
            Aggregates[aggregate.Id] = aggregate;
            return Task.CompletedTask;
        }

        public Task Delete(T aggregate, CancellationToken cancellationToken)
        {
            Aggregates.Remove(aggregate.Id);
            return Task.CompletedTask;
        }
    }
}
