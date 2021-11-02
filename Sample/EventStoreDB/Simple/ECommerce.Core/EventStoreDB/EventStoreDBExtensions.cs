using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Core.Serialisation;
using EventStore.Client;

namespace ECommerce.Core.EventStoreDB
{
    public static class EventStoreDBExtensions
    {
        public static async Task<TEntity> Find<TEntity>(
            this EventStoreClient eventStore,
            Func<TEntity> getDefault,
            Func<TEntity, object, TEntity> when,
            string id,
            CancellationToken cancellationToken)
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
                    getDefault(),
                    when,
                    cancellationToken
                ))!;
        }

        public static async Task Append(
            this EventStoreClient eventStore,
            string id,
            object @event,
            CancellationToken cancellationToken
        )

        {
            await eventStore.AppendToStreamAsync(
                id,
                StreamState.NoStream,
                new[] { @event.ToJsonEventData() },
                cancellationToken: cancellationToken
            );
        }


        public static async Task Append(
            this EventStoreClient eventStore,
            string id,
            object @event,
            uint version,
            CancellationToken cancellationToken
        )
        {
            await eventStore.AppendToStreamAsync(
                id,
                StreamRevision.FromInt64(version),
                new[] { @event.ToJsonEventData() },
                cancellationToken: cancellationToken
            );
        }
    }
}
