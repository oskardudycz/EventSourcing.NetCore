using EventStore.Client;

namespace Core.EventStoreDB.Subscriptions.Checkpoints;

public class Checkpoint
{
    public static Checkpoint None = new(null, null);

    public static Checkpoint From(ulong? value, ulong? storeRevision = null) => new(value, storeRevision);

    public static Checkpoint Reset(ulong? storeRevision = null) => new(null, storeRevision);


    private readonly ulong? position;

    public ulong Position =>
        position ?? throw new NullReferenceException("Checkpoint position is null!");

    public ulong? StoreRevision { get; }

    private Checkpoint(ulong? position, ulong? storeRevision)
    {
        this.position = position;
        StoreRevision = storeRevision ?? position;
    }

    public static implicit operator Position(Checkpoint checkpoint) =>
        new(checkpoint.Position, checkpoint.Position);

    protected bool Equals(Checkpoint other) =>
        position == other.position;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Checkpoint)obj);
    }

    public override int GetHashCode() => position.GetHashCode();

    public static bool operator ==(Checkpoint? obj1, Checkpoint? obj2)
        => obj1?.Equals((object?)obj2) ?? false;

    public static bool operator !=(Checkpoint? obj1, Checkpoint? obj2)
        => !(obj1 == obj2);
}
