namespace Core.EventStoreDB.Subscriptions.Checkpoints;

public interface ISubscriptionCheckpointRepository
{
    ValueTask<Checkpoint> Load(string subscriptionId, CancellationToken ct);

    ValueTask<StoreResult> Store(string subscriptionId, ulong position, Checkpoint previousCheckpoint, CancellationToken ct);

    ValueTask<Checkpoint> Reset(string subscriptionId, CancellationToken ct);

    public record StoreResult
    {
        public record Success(Checkpoint Checkpoint): StoreResult;

        public record Mismatch: StoreResult;
    }
}
