using System.Collections.Concurrent;

namespace Core.EventStoreDB.Subscriptions.Checkpoints;
using static ISubscriptionCheckpointRepository;

public class InMemorySubscriptionCheckpointRepository: ISubscriptionCheckpointRepository
{
    private readonly ConcurrentDictionary<string, Checkpoint> checkpoints = new();

    public ValueTask<Checkpoint> Load(string subscriptionId, CancellationToken ct)
    {
        return new ValueTask<Checkpoint>(
            checkpoints.TryGetValue(subscriptionId, out var checkpoint) ? checkpoint : Checkpoint.None
        );
    }

    public ValueTask<StoreResult> Store(string subscriptionId, ulong position, Checkpoint previousPosition,
        CancellationToken ct)
    {
        var checkpoint = Checkpoint.From(position);

        checkpoints.AddOrUpdate(subscriptionId, checkpoint, (_, current) =>
            // Don't update if position doesn't match
            (current != previousPosition) ? checkpoint : current
        );

        return new ValueTask<StoreResult>(new StoreResult.Success(checkpoint));
    }

    public ValueTask<Checkpoint> Reset(string subscriptionId, CancellationToken ct)
    {
        var checkpoint = Checkpoint.None;

        checkpoints.AddOrUpdate(subscriptionId, checkpoint, (_, _) => checkpoint);

        return new ValueTask<Checkpoint>(checkpoint);
    }
}
