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
        Task<T> Find(Guid id, Func<T, object, T> when, CancellationToken cancellationToken);
        Task Append(Guid id, object @event, CancellationToken cancellationToken);
    }

    public class EventStoreDBRepository<T>: IEventStoreDBRepository<T> where T: notnull
    {
        private readonly EventStoreClient eventStore;

        public EventStoreDBRepository(
            EventStoreClient eventStore
        )
        {
            this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        }

        public async Task<T> Find(Guid id, Func<T, object, T> when, CancellationToken cancellationToken)
        {
            var readResult = eventStore.ReadStreamAsync(
                Direction.Forwards,
                StreamNameMapper.ToStreamId<T>(id),
                StreamPosition.Start,
                cancellationToken: cancellationToken
            );

            var aggregate = (T)Activator.CreateInstance(typeof(T), true)!;

            return await readResult
                .Select(@event => @event.Deserialize())
                .AggregateAsync(
                    aggregate,
                    when,
                    cancellationToken
                );
        }

        public async Task Append(Guid id, object @event, CancellationToken cancellationToken)
        {
            await eventStore.AppendToStreamAsync(
                StreamNameMapper.ToStreamId<T>(id),
                // TODO: Add proper optimistic concurrency handling
                StreamState.Any,
                new[] { @event.ToJsonEventData() },
                cancellationToken: cancellationToken
            );
        }
    }
}
