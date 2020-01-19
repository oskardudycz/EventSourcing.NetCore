using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Aggregates;
using Core.Events;
using Core.Storage;
using Marten;

namespace Core.Storage
{
    public class MartenRepository<T>: IRepository<T> where T : class, IAggregate, new()
    {
        private readonly IDocumentSession documentSession;

        public MartenRepository(
            IDocumentSession documentSession
        )
        {
            this.documentSession = documentSession ?? throw new ArgumentNullException(nameof(documentSession));
        }

        public Task<T> Find(Guid id, CancellationToken cancellationToken)
        {
            return documentSession.Events.AggregateStreamAsync<T>(id);
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
            var events = aggregate.DequeueUncommittedEvents();
            documentSession.Events.Append(
                aggregate.Id,
                events.ToArray()
            );
            await documentSession.SaveChangesAsync(cancellationToken);
        }
    }
}
