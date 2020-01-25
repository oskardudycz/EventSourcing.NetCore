using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Aggregates;
using Marten;
using Core.Events;

namespace Core.Storage
{
    public class MartenRepository<T>: IRepository<T> where T : class, IAggregate, new()
    {
        private readonly IDocumentSession documentSession;
        private readonly IEventBus eventBus;

        public MartenRepository(
            IDocumentSession documentSession,
            IEventBus eventBus
        )
        {
            Guard.Against.Null(documentSession, nameof(documentSession));
            Guard.Against.Null(eventBus, nameof(eventBus));

            this.documentSession = documentSession;
            this.eventBus = eventBus;
        }

        public Task<T> Find(Guid id, CancellationToken cancellationToken)
        {
            return documentSession.Events.AggregateStreamAsync<T>(id, token:cancellationToken);
        }

        public Task Add(T aggregate, CancellationToken cancellationToken)
        {
            return Store(aggregate, cancellationToken);
        }

        public Task Update(T aggregate, CancellationToken cancellationToken)
        {
            return Store(aggregate, cancellationToken);
        }

        public Task Delete(T aggregate, CancellationToken cancellationToken)
        {
            return Store(aggregate, cancellationToken);
        }

        private async Task Store(T aggregate, CancellationToken cancellationToken)
        {
            var events = aggregate.DequeueUncommittedEvents().ToArray();
            documentSession.Events.Append(
                aggregate.Id,
                events
            );
            await documentSession.SaveChangesAsync(cancellationToken);
            await eventBus.Publish(events);
        }
    }
}
