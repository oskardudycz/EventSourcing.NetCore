using Core.EventStoreDB.Subscriptions.Checkpoints;
using EventStore.Client;

namespace Core.EventStoreDB.Subscriptions.Batch;
using static ISubscriptionCheckpointRepository;

public interface IEventsBatchCheckpointer
{
    Task<StoreResult> Process(
        ResolvedEvent[] events,
        Checkpoint lastCheckpoint,
        EventStoreDBSubscriptionToAllOptions subscriptionOptions,
        CancellationToken ct
    );
}

public class EventsBatchCheckpointer(
    ISubscriptionCheckpointRepository checkpointRepository,
    EventsBatchProcessor eventsBatchProcessor
): IEventsBatchCheckpointer
{
    public async Task<StoreResult> Process(
        ResolvedEvent[] events,
        Checkpoint lastCheckpoint,
        EventStoreDBSubscriptionToAllOptions subscriptionOptions,
        CancellationToken ct
    )
    {
        var lastPosition = events.LastOrDefault().OriginalPosition?.CommitPosition;

        if (!lastPosition.HasValue)
            return new StoreResult.Ignored();

        await eventsBatchProcessor.HandleEventsBatch(events, subscriptionOptions, ct)
            .ConfigureAwait(false);

        return await checkpointRepository
            .Store(subscriptionOptions.SubscriptionId, lastPosition.Value, lastCheckpoint, ct)
            .ConfigureAwait(false);
    }
}
