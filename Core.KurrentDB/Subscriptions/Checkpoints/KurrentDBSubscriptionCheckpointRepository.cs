using Core.KurrentDB.Serialization;
using KurrentDB.Client;

namespace Core.KurrentDB.Subscriptions.Checkpoints;

using static ISubscriptionCheckpointRepository;

public record CheckpointStored(string SubscriptionId, ulong? Position, DateTime CheckpointedAt);

public class KurrentDBSubscriptionCheckpointRepository(KurrentDBClient kurrentDBClient)
    : ISubscriptionCheckpointRepository
{
    private readonly KurrentDBClient kurrentDBClient =
        kurrentDBClient ?? throw new ArgumentNullException(nameof(kurrentDBClient));

    public async ValueTask<Checkpoint> Load(string subscriptionId, CancellationToken ct)
    {
        var streamName = GetCheckpointStreamName(subscriptionId);

        var readResult = kurrentDBClient.ReadStreamAsync(Direction.Backwards, streamName, StreamPosition.End, 1,
            cancellationToken: ct);

        if (await readResult.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            return Checkpoint.None;

        await foreach (var resolvedEvent in readResult)
            return Checkpoint.From(
                resolvedEvent.Deserialize<CheckpointStored>()?.Position,
                resolvedEvent.Event.EventNumber
            );

        throw new InvalidOperationException("Didn't find Checkpoint!");
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
            await kurrentDBClient.SetStreamMetadataAsync(
                streamName,
                // Using Any instead of NoStream, as KurrentDB is not doing transactional changes.
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
                ? await kurrentDBClient.AppendToStreamAsync(
                    streamName,
                    StreamState.StreamRevision(previousCheckpoint.StoreRevision.Value),
                    eventToAppend,
                    cancellationToken: ct
                ).ConfigureAwait(false)
                : await kurrentDBClient.AppendToStreamAsync(
                    streamName,
                    StreamState.NoStream,
                    eventToAppend,
                    cancellationToken: ct
                ).ConfigureAwait(false);

            return new StoreResult.Success(
                Checkpoint.From(position, (ulong)result.NextExpectedStreamState.ToInt64())
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

        var result = await kurrentDBClient.AppendToStreamAsync(
            streamName,
            StreamState.Any,
            eventToAppend,
            cancellationToken: ct
        ).ConfigureAwait(false);

        return Checkpoint.Reset((ulong)result.NextExpectedStreamState.ToInt64());
    }

    private static string GetCheckpointStreamName(string subscriptionId) => $"checkpoint_{subscriptionId}";
}
