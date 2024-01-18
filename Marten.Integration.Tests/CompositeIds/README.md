
# Strongly-Typed ids with Marten

Strongly typed ids (or, in general, a proper type system) can make your code more predictable. It reduces the chance of trivial mistakes, like accidentally changing parameters order of the same primitive type. 

So for such code:

```csharp
var reservationId = "RES/01";
var seatId = "SEAT/22";
var customerId = "CUS/291";

var reservation = new Reservation(
    reservationId,
    seatId,
    customerId 
);
```

the compiler won't catch if you switch `reservationId` with `seatId`.

If you use strongly typed ids, then compile will catch that issue:

```csharp
var reservationId = new ReservationId("RES/01");
var seatId = new SeatId("SEAT/22");
var customerId = new CustomerId("CUS/291");

var reservation = new Reservation(
    reservationId,
    seatId,
    customerId 
);
```

They're not ideal, as they're usually not playing well with the storage engines. Typical issues are: serialisation, Linq queries, etc. For some cases they may be just overkill. You need to pick your poison.

To reduce tedious, copy/paste code, it's worth defining a strongly-typed id base class, like:

```csharp
public class StronglyTypedValue<T>: IEquatable<StronglyTypedValue<T>> where T: IComparable<T>
{
    public T Value { get; }

    public StronglyTypedValue(T value)
    {
        Value = value;
    }

    public bool Equals(StronglyTypedValue<T>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((StronglyTypedValue<T>)obj);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(Value);
    }

    public static bool operator ==(StronglyTypedValue<T>? left, StronglyTypedValue<T>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(StronglyTypedValue<T>? left, StronglyTypedValue<T>? right)
    {
        return !Equals(left, right);
    }
}
```

Then you can define specific id class as:

```csharp
public class ReservationId: StronglyTypedValue<Guid>
{
    public ReservationId(Guid value) : base(value)
    {
    }
}
```

You can even add additional rules:

```csharp
public class ReservationNumber: StronglyTypedValue<string>
{
    public ReservationNumber(string value) : base(value)
    {
        if (string.IsNullOrEmpty(value) || value.StartsWith("RES/") || value.Length <= 4)
            throw new ArgumentOutOfRangeException(nameof(value));
    }
}
```

The base class working with Marten, can be defined as:

```csharp
public abstract class Aggregate<TKey, T>
    where TKey: StronglyTypedValue<T>
    where T : IComparable<T>
{
    public TKey Id { get; set; } = default!;
    
    [Identity]
    public T AggregateId    {
        get => Id.Value;
        set {} 
    }

    public int Version { get; protected set; }

    [JsonIgnore] private readonly Queue<object> uncommittedEvents = new(); 

    public object[] DequeueUncommittedEvents()
    {
        var dequeuedEvents = uncommittedEvents.ToArray();

        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    protected void Enqueue(object @event)
    {
        uncommittedEvents.Enqueue(@event);
    }
}
```

Marten requires the id with public setter and getter of `string` or `Guid`. We used the trick and added `AggregateId` with a strongly-typed backing field. We also informed Marten of the [Identity](https://martendb.io/documents/identity.html#document-identity) attribute to use this field in its internals.

Example aggregate can look like:

```csharp
public class Reservation : Aggregate<ReservationId, Guid>
{
    public CustomerId CustomerId { get; private set; } = default!;

    public SeatId SeatId { get; private set; } = default!;

    public ReservationNumber Number { get; private set; } = default!;

    public ReservationStatus Status { get; private set; }

    public static Reservation CreateTentative(
        SeatId seatId,
        CustomerId customerId)
    {
        return new Reservation(
            new ReservationId(Guid.NewGuid()),
            seatId,
            customerId,
            new ReservationNumber(Guid.NewGuid().ToString())
        );
    }

    // (...)
}
```

See the full sample [here](./CompositeIdsTests.cs).

Read more in the article:
-   📝 [Using strongly-typed identifiers with Marten](https://event-driven.io/en/using_strongly_typed_ids_with_marten//?utm_source=event_sourcing_net)
-   📝 [Immutable Value Objects are simpler and more useful than you think!](https://event-driven.io/en/immutable_value_objects/?utm_source=event_sourcing_net)
