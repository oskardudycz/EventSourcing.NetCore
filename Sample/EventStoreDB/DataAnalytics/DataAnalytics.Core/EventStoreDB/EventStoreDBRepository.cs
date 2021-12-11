using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAnalytics.Core.Serialisation;
using EventStore.Client;

namespace DataAnalytics.Core.Entities
{
    public static class EventStoreDBRepository
    {
        public static async Task<TEntity?> AggregateStream<TEntity>(
            this EventStoreClient eventStore,
            Func<TEntity?, object, TEntity> when,
            string id,
            CancellationToken cancellationToken
        ) where TEntity: class
        {
            var result = eventStore.ReadStreamAsync(
                Direction.Forwards,
                id,
                StreamPosition.Start,
                cancellationToken: cancellationToken
            );

            if (await result.ReadState == ReadState.StreamNotFound) {
                return null;
            }


            return (await result
                .Select(@event => @event.DeserializeData())
                .AggregateAsync(
                    default,
                    when,
                    cancellationToken
                ))!;
        }

        public static async Task<TEvent?> ReadLastEvent<TEvent>(
            this EventStoreClient eventStore,
            string id,
            CancellationToken ct
        ) where TEvent: class
        {
            var resolvedEvent = await eventStore.ReadLastEvent(id, ct);

            return resolvedEvent?.DeserializeData<TEvent>();
        }

        public static async Task<ResolvedEvent?> ReadLastEvent(
            this EventStoreClient eventStore,
            string id,
            CancellationToken ct
        )
        {
            var result = eventStore.ReadStreamAsync(
                Direction.Backwards,
                id,
                StreamPosition.End,
                maxCount: 1,
                cancellationToken: ct
            );

            if (await result.ReadState == ReadState.StreamNotFound) {
                return null;
            }

            return await result.FirstAsync(ct);
        }

        public static async Task AppendToNewStream(
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

        public static async Task AppendToStreamWithSingleEvent(
            this EventStoreClient eventStore,
            string id,
            object @event,
            CancellationToken ct
        )
        {
            var eventData = new[] { @event.ToJsonEventData() };

            var result = await eventStore.AppendToStreamAsync(
                id,
                StreamState.StreamExists,
                eventData,
                options => {
                    options.ThrowOnAppendFailure = false;
                },
                cancellationToken: ct
            );

            if (result is SuccessResult)
                return;

            await eventStore.SetStreamMetadataAsync(
                id,
                StreamState.NoStream,
                new StreamMetadata(maxCount:1),
                cancellationToken: ct
            );

            await eventStore.AppendToStreamAsync(
                id,
                StreamState.NoStream,
                eventData,
                cancellationToken: ct
            );
        }

        public static async Task AppendToExisting(
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

        public static async Task Append(
            this EventStoreClient eventStore,
            string id,
            object @event,
            CancellationToken cancellationToken
        )
        {
            await eventStore.AppendToStreamAsync(
                id,
                StreamState.Any,
                new[] { @event.ToJsonEventData() },
                cancellationToken: cancellationToken
            );
        }
    }
}
