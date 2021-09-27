using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using ECommerce.Core.Serialisation;
using EventStore.Client;

namespace ECommerce.Core.Entities
{
    public interface IEventStoreDBRepository<T> where T: notnull
    {
        Task<T> Find(Func<T?, object, T> when, string id, CancellationToken cancellationToken);
        Task Append(string id, object @event, CancellationToken cancellationToken);
    }

    public class EventStoreDBRepository<T>: IEventStoreDBRepository<T> where T: class
    {
        private readonly EventStoreClient eventStore;

        public EventStoreDBRepository(
            EventStoreClient eventStore
        )
        {
            this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        }

        public async Task<T> Find(Func<T?, object, T> when, string id, CancellationToken cancellationToken)
        {
            var readResult = eventStore.ReadStreamAsync(
                Direction.Forwards,
                id,
                StreamPosition.Start,
                cancellationToken: cancellationToken
            );

            return (await readResult
                .Select(@event => @event.Deserialize())
                .AggregateAsync(
                    default,
                    when,
                    cancellationToken
                ))!;
        }

        public async Task Append(string id, object @event, CancellationToken cancellationToken)
        {
            await eventStore.AppendToStreamAsync(
                id,
                // TODO: Add proper optimistic concurrency handling
                StreamState.Any,
                new[] { @event.ToJsonEventData() },
                cancellationToken: cancellationToken
            );
        }
    }
}
