using Core.EventStoreDB.Serialization;
using EventStore.Client;

namespace Core.EventStoreDB.Subscriptions.Checkpoints;

using static ISubscriptionCheckpointRepository;

public record CheckpointStored(string SubscriptionId, ulong? Position, DateTime CheckpointedAt);

public class EventStoreDBSubscriptionCheckpointRepository(EventStoreClient eventStoreClient)
    : ISubscriptionCheckpointRepository
{
    private readonly EventStoreClient eventStoreClient =
        eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));

    public async ValueTask<Checkpoint> Load(string subscriptionId, CancellationToken ct)
    {
        var streamName = GetCheckpointStreamName(subscriptionId);

        var result = eventStoreClient.ReadStreamAsync(Direction.Backwards, streamName, StreamPosition.End, 1,
            cancellationToken: ct);

        if (await result.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            return Checkpoint.None;

        ResolvedEvent? resolvedEvent = await result.FirstOrDefaultAsync(ct).ConfigureAwait(false);

        return Checkpoint.From(
            resolvedEvent?.Deserialize<CheckpointStored>()?.Position,
            resolvedEvent?.Event.EventNumber
        );
    }

    public async ValueTask<StoreResult> Store(
        string subscriptionId,
        ulong position,
        Checkpoint previousCheckpoint,
        CancellationToken ct)
    {
        var @event = new CheckpointStored(subscriptionId, position, DateTime.UtcNow);

        var eventToAppend = new[] { @event.ToJsonEventData() };
        var streamName = GetCheckpointStreamName(subscriptionId);

        if (previousCheckpoint == Checkpoint.None)
        {
            await eventStoreClient.SetStreamMetadataAsync(
                streamName,
                // Using Any instead of NoStream, as EventStoreDB is not doing transactional changes.
                // In case of race condition where we added metadata, but AppendToStream failed,
                // If we used NoStream then calling it again we'd end up in the endless failures.
                // Setting stream metadata more than once won't do us much harm as this should only happen in the edge case.
                StreamState.Any,
                new StreamMetadata(1),
                cancellationToken: ct
            ).ConfigureAwait(false);
        }

        try
        {
            var result = previousCheckpoint.StoreRevision.HasValue
                ? await eventStoreClient.AppendToStreamAsync(
                    streamName,
                    StreamRevision.FromInt64((long)previousCheckpoint.StoreRevision.Value),
                    eventToAppend,
                    cancellationToken: ct
                ).ConfigureAwait(false)
                : await eventStoreClient.AppendToStreamAsync(
                    streamName,
                    StreamState.NoStream,
                    eventToAppend,
                    cancellationToken: ct
                ).ConfigureAwait(false);

            return new StoreResult.Success(
                Checkpoint.From(position, (ulong)result.NextExpectedStreamRevision.ToInt64())
            );
        }
        catch (WrongExpectedVersionException)
        {
            return new StoreResult.Mismatch();
        }
    }

    public async ValueTask<Checkpoint> Reset(string subscriptionId, CancellationToken ct)
    {
        var @event = new CheckpointStored(subscriptionId, null, DateTime.UtcNow);
        var eventToAppend = new[] { @event.ToJsonEventData() };
        var streamName = GetCheckpointStreamName(subscriptionId);

        var result = await eventStoreClient.AppendToStreamAsync(
            streamName,
            StreamState.Any,
            eventToAppend,
            cancellationToken: ct
        ).ConfigureAwait(false);

        return Checkpoint.Reset(result.NextExpectedStreamRevision);
    }

    private static string GetCheckpointStreamName(string subscriptionId) => $"checkpoint_{subscriptionId}";
}
