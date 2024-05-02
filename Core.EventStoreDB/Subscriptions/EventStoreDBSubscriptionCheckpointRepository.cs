﻿using Core.EventStoreDB.Serialization;
using EventStore.Client;

namespace Core.EventStoreDB.Subscriptions;

public record CheckpointStored(string SubscriptionId, ulong? Position, DateTime CheckpointedAt);

public class EventStoreDBSubscriptionCheckpointRepository(EventStoreClient eventStoreClient)
    : ISubscriptionCheckpointRepository
{
    private readonly EventStoreClient eventStoreClient = eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));

    public async ValueTask<ulong?> Load(string subscriptionId, CancellationToken ct)
    {
        var streamName = GetCheckpointStreamName(subscriptionId);

        var result = eventStoreClient.ReadStreamAsync(Direction.Backwards, streamName, StreamPosition.End, 1,
            cancellationToken: ct);

        if (await result.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
        {
            return null;
        }

        ResolvedEvent? @event = await result.FirstOrDefaultAsync(ct).ConfigureAwait(false);

        return @event?.Deserialize<CheckpointStored>()?.Position;
    }

    public async ValueTask Store(string subscriptionId, ulong position, CancellationToken ct)
    {
        var @event = new CheckpointStored(subscriptionId, position, DateTime.UtcNow);
        var eventToAppend = new[] {@event.ToJsonEventData()};
        var streamName = GetCheckpointStreamName(subscriptionId);

        try
        {
            // store new checkpoint expecting stream to exist
            await eventStoreClient.AppendToStreamAsync(
                streamName,
                StreamState.StreamExists,
                eventToAppend,
                cancellationToken: ct
            ).ConfigureAwait(false);
        }
        catch (WrongExpectedVersionException)
        {
            // WrongExpectedVersionException means that stream did not exist
            // Set the checkpoint stream to have at most 1 event
            // using stream metadata $maxCount property
            await eventStoreClient.SetStreamMetadataAsync(
                streamName,
                StreamState.NoStream,
                new StreamMetadata(1),
                cancellationToken: ct
            ).ConfigureAwait(false);

            // append event again expecting stream to not exist
            await eventStoreClient.AppendToStreamAsync(
                streamName,
                StreamState.NoStream,
                eventToAppend,
                cancellationToken: ct
            ).ConfigureAwait(false);
        }
    }

    private static string GetCheckpointStreamName(string subscriptionId) => $"checkpoint_{subscriptionId}";
}
