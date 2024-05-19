using Core.EventStoreDB.Subscriptions.Checkpoints;
using Core.Extensions;
using EventStore.Client;

namespace Core.EventStoreDB.Subscriptions.Batch;

using static EventStoreClient;

public class EventsBatchCheckpointer(
    ISubscriptionCheckpointRepository checkpointRepository,
    EventsBatchProcessor eventsBatchProcessor
)
{
    public async Task<Checkpoint> Process(
        ResolvedEvent[] events,
        Checkpoint lastCheckpoint,
        EventStoreDBSubscriptionToAllOptions subscriptionOptions,
        CancellationToken ct
    )
    {
        var processedPosition = await eventsBatchProcessor.HandleEventsBatch(events, subscriptionOptions, ct)
            .ConfigureAwait(false);

        if (!processedPosition.HasValue)
            return lastCheckpoint;

        var result = await checkpointRepository
            .Store(subscriptionOptions.SubscriptionId, processedPosition.Value, lastCheckpoint, ct)
            .ConfigureAwait(false);

        if (result is not ISubscriptionCheckpointRepository.StoreResult.Success success)
            throw new InvalidOperationException(
                $"Mismatch while updating Store checkpoint! Ensure that you don't have multiple instances running Subscription with id: {subscriptionOptions.SubscriptionId}");

        return success.Checkpoint;
    }
}
