using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using Core.EventStoreDB.Serialization;
using Core.Subscriptions;
using EventStore.Client;

namespace Core.EventStoreDB.Subscriptions
{
    public record CheckpointStored(string SubscriptionId, ulong? Position, DateTime CheckpointedAt): IEvent;

    public class EventStoreDBSubscriptionCheckpointRepository: ISubscriptionCheckpointRepository
    {
        private readonly EventStoreClient eventStoreClient;

        public EventStoreDBSubscriptionCheckpointRepository(
            EventStoreClient eventStoreClient)
        {
            this.eventStoreClient = eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));
        }

        public async ValueTask<ulong?> Load(string subscriptionId, CancellationToken ct)
        {
            var streamName = GetCheckpointStreamName(subscriptionId);

            var result = eventStoreClient.ReadStreamAsync(Direction.Backwards, streamName, StreamPosition.End, 1,
                cancellationToken: ct);

            if (await result.ReadState == ReadState.StreamNotFound)
            {
                return null;
            }

            ResolvedEvent? @event = await result.FirstOrDefaultAsync(ct);

            return @event?.Deserialize<CheckpointStored>().Position;
        }

        public async ValueTask Store(string subscriptionId, ulong position, CancellationToken ct)
        {
            var @event = new CheckpointStored(subscriptionId, position, DateTime.UtcNow);
            var eventToAppend = new[] {@event.ToJsonEventData()};
            var streamName = GetCheckpointStreamName(subscriptionId);

            try
            {
                await eventStoreClient.AppendToStreamAsync(
                    streamName,
                    StreamState.StreamExists,
                    eventToAppend,
                    cancellationToken: ct
                );
            }
            catch (WrongExpectedVersionException)
            {
                await eventStoreClient.SetStreamMetadataAsync(
                    streamName,
                    StreamState.NoStream,
                    new StreamMetadata(1),
                    cancellationToken: ct
                );

                await eventStoreClient.AppendToStreamAsync(
                    streamName,
                    StreamState.NoStream,
                    eventToAppend,
                    cancellationToken: ct
                );
            }
        }

        private static string GetCheckpointStreamName(string subscriptionId) => $"checkpoint_{subscriptionId}";
    }
}
