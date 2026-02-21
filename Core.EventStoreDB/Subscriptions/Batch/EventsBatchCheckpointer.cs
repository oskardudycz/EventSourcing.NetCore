using Core.EventStoreDB.Subscriptions.Checkpoints;
using EventStore.Client;

namespace Core.EventStoreDB.Subscriptions.Batch;
using static ISubscriptionCheckpointRepository;

public interface IEventsBatchCheckpointer
{
    Task<StoreResult> Process(
        ResolvedEvent[] events,
        Checkpoint lastCheckpoint,
        BatchProcessingOptions batchProcessingOptions,
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
        BatchProcessingOptions options,
        CancellationToken ct
    )
    {
        var lastPosition = events.LastOrDefault().OriginalPosition?.CommitPosition;

        if (!lastPosition.HasValue)
            return new StoreResult.Ignored();

        await eventsBatchProcessor.Handle(events, options, ct)
            .ConfigureAwait(false);

        return await checkpointRepository
            .Store(options.SubscriptionId, lastPosition.Value, lastCheckpoint, ct)
            .ConfigureAwait(false);
    }
}
