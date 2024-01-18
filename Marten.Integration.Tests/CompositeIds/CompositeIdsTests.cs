using FluentAssertions;
using Marten.Events.Projections;
using Marten.Integration.Tests.TestsInfrastructure;
using Marten.Schema;
using Newtonsoft.Json;
using Weasel.Core;
using Xunit;

namespace Marten.Integration.Tests.CompositeIds;

public class StronglyTypedValue<T>: IEquatable<StronglyTypedValue<T>> where T : IComparable<T>
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
        if (obj.GetType() != GetType()) return false;
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

public class ReservationId: StronglyTypedValue<Guid>
{
    public ReservationId(Guid value): base(value)
    {
    }
}

public class CustomerId: StronglyTypedValue<Guid>
{
    public CustomerId(Guid value): base(value)
    {
    }
}

public class SeatId: StronglyTypedValue<Guid>
{
    public SeatId(Guid value): base(value)
    {
    }
}

public class ReservationNumber: StronglyTypedValue<string>
{
    public ReservationNumber(string value): base(value)
    {
        if (string.IsNullOrEmpty(value) || value.StartsWith("RES/") || value.Length <= 4)
            throw new ArgumentOutOfRangeException(nameof(value));
    }
}

public record TentativeReservationCreated(
    ReservationId ReservationId,
    SeatId SeatId,
    CustomerId CustomerId,
    ReservationNumber Number
);

public record ReservationSeatChanged(
    ReservationId ReservationId,
    SeatId SeatId
);

public record ReservationConfirmed(
    ReservationId ReservationId
);

public record ReservationCancelled(
    ReservationId ReservationId
);

public abstract class Aggregate<TKey, T>
    where TKey : StronglyTypedValue<T>
    where T : IComparable<T>
{
    public TKey Id { get; set; } = default!;

    [Identity]
    public T AggregateId
    {
        get => Id.Value;
        set { }
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

public enum ReservationStatus
{
    Tentative,
    Confirmed,
    Cancelled
}

public class Reservation: Aggregate<ReservationId, Guid>
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

    private Reservation() { }

    private Reservation(
        ReservationId id,
        SeatId seatId,
        CustomerId customerId,
        ReservationNumber reservationNumber
    )
    {
        var @event = new TentativeReservationCreated(
            id,
            seatId,
            customerId,
            reservationNumber
        );

        Enqueue(@event);
        Apply(@event);
    }


    public void ChangeSeat(SeatId newSeatId)
    {
        if (Status != ReservationStatus.Tentative)
            throw new InvalidOperationException(
                $"Changing seat for the reservation in '{Status}' status is not allowed.");

        var @event = new ReservationSeatChanged(Id, newSeatId);

        Enqueue(@event);
        Apply(@event);
    }

    public void Confirm()
    {
        if (Status != ReservationStatus.Tentative)
            throw new InvalidOperationException(
                $"Only tentative reservation can be confirmed (current status: {Status}.");

        var @event = new ReservationConfirmed(Id);

        Enqueue(@event);
        Apply(@event);
    }

    public void Cancel()
    {
        if (Status != ReservationStatus.Tentative)
            throw new InvalidOperationException(
                $"Only tentative reservation can be cancelled (current status: {Status}).");

        var @event = new ReservationCancelled(Id);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(TentativeReservationCreated @event)
    {
        Id = @event.ReservationId;
        SeatId = @event.SeatId;
        CustomerId = @event.CustomerId;
        Number = @event.Number;
        Status = ReservationStatus.Tentative;
    }

    public void Apply(ReservationSeatChanged @event)
    {
        SeatId = @event.SeatId;
    }

    public void Apply(ReservationConfirmed @event)
    {
        Status = ReservationStatus.Confirmed;
    }

    public void Apply(ReservationCancelled @event)
    {
        Status = ReservationStatus.Cancelled;
    }
}

public class CompositeIdsTests: MartenTest
{
    private const string FirstTenant = "Tenant1";
    private const string SecondTenant = "Tenant2";

    protected override IDocumentSession CreateSession(Action<StoreOptions>? setStoreOptions = null)
    {
        var store = DocumentStore.For(options =>
        {
            options.Connection(ConnectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.DatabaseSchemaName = SchemaName;
            options.Events.DatabaseSchemaName = SchemaName;
            options.UseDefaultSerialization(nonPublicMembersStorage: NonPublicMembersStorage.All);

            options.Projections.Snapshot<Reservation>(SnapshotLifecycle.Inline);
        });

        return store.LightweightSession();
    }

    [Fact]
    public void GivenAggregateWithCompositeId_WhenAppendedEvent_LiveAndInlineAggregationWorks()
    {
        var seatId = new SeatId(Guid.NewGuid());
        var customerId = new CustomerId(Guid.NewGuid());

        var reservation = Reservation.CreateTentative(seatId, customerId);
        var @event = reservation.DequeueUncommittedEvents().Single();

        //1. Create events
        EventStore.Append(reservation.AggregateId, @event);

        Session.SaveChanges();

        //2. Get live aggregation
        var issuesListFromLiveAggregation = EventStore.AggregateStream<Reservation>(reservation.AggregateId);

        //3. Get inline aggregation
        var issuesListFromInlineAggregation = Session.Load<Reservation>(reservation.AggregateId);

        //4. Get inline aggregation through linq
        var reservationId = reservation.Id;

        var issuesListFromInlineAggregationFromLinq =
            Session.Query<Reservation>().SingleOrDefault(r => r.Id.Value == reservationId.Value);
        var issuesListFromInlineAggregationFromLinqWithAggregateId = Session.Query<Reservation>()
            .SingleOrDefault(r => r.AggregateId == reservationId.Value);

        issuesListFromLiveAggregation.Should().NotBeNull();
        issuesListFromInlineAggregation.Should().NotBeNull();
        issuesListFromInlineAggregationFromLinq.Should().NotBeNull();
        issuesListFromInlineAggregationFromLinqWithAggregateId.Should().NotBeNull();

        issuesListFromLiveAggregation!.AggregateId.Should().Be(reservationId.Value);
        issuesListFromLiveAggregation!.AggregateId.Should().Be(reservationId.Value);
        issuesListFromInlineAggregationFromLinq!.AggregateId.Should().Be(reservationId.Value);
        issuesListFromInlineAggregationFromLinqWithAggregateId!.AggregateId.Should().Be(reservationId.Value);
    }

    public CompositeIdsTests(MartenFixture fixture): base(fixture.PostgreSqlContainer)
    {
    }
}
