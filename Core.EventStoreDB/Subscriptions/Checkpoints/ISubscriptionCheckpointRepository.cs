namespace Core.EventStoreDB.Subscriptions.Checkpoints;

public interface ISubscriptionCheckpointRepository
{
    ValueTask<Checkpoint> Load(string subscriptionId, CancellationToken ct);

    ValueTask<StoreResult> Store(string subscriptionId, ulong position, Checkpoint previousCheckpoint, CancellationToken ct);

    ValueTask<Checkpoint> Reset(string subscriptionId, CancellationToken ct);

    public record StoreResult
    {
        public record Success(Checkpoint Checkpoint): StoreResult;
        public record Ignored: StoreResult;
        public record Mismatch: StoreResult;
    }
}


public interface ISubscriptionStoreSetup
{
    ValueTask EnsureStoreExists(CancellationToken ct);
}

public class NulloSubscriptionStoreSetup: ISubscriptionStoreSetup
{
    public ValueTask EnsureStoreExists(CancellationToken ct) =>
        ValueTask.CompletedTask;
}
