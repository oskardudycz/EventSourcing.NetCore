using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Marten.Integration.Tests.TestsInfrastructure;
using Newtonsoft.Json;
using Weasel.Postgresql;
using Xunit;

namespace Marten.Integration.Tests.CompositeIds;

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

public class ReservationId: StronglyTypedValue<Guid>
{
    public ReservationId(Guid value) : base(value)
    {
    }
};

public class CustomerId: StronglyTypedValue<Guid>
{
    public CustomerId(Guid value) : base(value)
    {
    }
};

public class SeatId: StronglyTypedValue<Guid>
{
    public SeatId(Guid value) : base(value)
    {
    }
};

public class ReservationNumber: StronglyTypedValue<string>
{
    public ReservationNumber(string value) : base(value)
    {
    }
};

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
    where TKey: StronglyTypedValue<T>
    where T : IComparable<T>
{

    public TKey AggregateId { get; set; } = default!;

    public T Id
    {
        get => AggregateId.Value;
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

public enum ReservationStatus
{
    Tentative,
    Confirmed,
    Cancelled
}

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

    private Reservation(){}

    private Reservation(
        ReservationId aggregateId,
        SeatId seatId,
        CustomerId customerId,
        ReservationNumber reservationNumber
    )
    {
        var @event = new TentativeReservationCreated(
            aggregateId,
            seatId,
            customerId,
            reservationNumber
        );

        Enqueue(@event);
        Apply(@event);
    }


    public void ChangeSeat(SeatId newSeatId)
    {
        if(Status != ReservationStatus.Tentative)
            throw new InvalidOperationException($"Changing seat for the reservation in '{Status}' status is not allowed.");

        var @event = new ReservationSeatChanged(AggregateId, newSeatId);

        Enqueue(@event);
        Apply(@event);
    }

    public void Confirm()
    {
        if(Status != ReservationStatus.Tentative)
            throw new InvalidOperationException($"Only tentative reservation can be confirmed (current status: {Status}.");

        var @event = new ReservationConfirmed(AggregateId);

        Enqueue(@event);
        Apply(@event);
    }

    public void Cancel()
    {
        if(Status != ReservationStatus.Tentative)
            throw new InvalidOperationException($"Only tentative reservation can be cancelled (current status: {Status}).");

        var @event = new ReservationCancelled(AggregateId);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(TentativeReservationCreated @event)
    {
        AggregateId = @event.ReservationId;
        SeatId = @event.SeatId;
        CustomerId = @event.CustomerId;
        Number = @event.Number;
        Status = ReservationStatus.Tentative;
        Version++;
    }

    public void Apply(ReservationSeatChanged @event)
    {
        SeatId = @event.SeatId;
        Version++;
    }

    public void Apply(ReservationConfirmed @event)
    {
        Status = ReservationStatus.Confirmed;
        Version++;
    }

    public void Apply(ReservationCancelled @event)
    {
        Status = ReservationStatus.Cancelled;
        Version++;
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
            options.Connection(Settings.ConnectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.DatabaseSchemaName = SchemaName;
            options.Events.DatabaseSchemaName = SchemaName;
            options.UseDefaultSerialization(nonPublicMembersStorage: NonPublicMembersStorage.All);

            options.Projections.SelfAggregate<Reservation>();
        });

        return store.OpenSession();
    }

    [Fact]
    public void GivenAggregateWithCompositeId_WhenAppendedEvent_LiveAndInlineAggregationWorks()
    {
        var seatId = new SeatId(Guid.NewGuid());
        var customerId = new CustomerId(Guid.NewGuid());

        var reservation = Reservation.CreateTentative(seatId, customerId);
        var @event = reservation.DequeueUncommittedEvents().Single();

        //1. Create events
        EventStore.Append(reservation.Id, @event);

        Session.SaveChanges();

        //2. Get live agregation
        var issuesListFromLiveAggregation = EventStore.AggregateStream<Reservation>(reservation.Id);

        //3. Get inline aggregation
        var issuesListFromInlineAggregation = Session.Load<Reservation>(reservation.Id);

        issuesListFromLiveAggregation.Should().NotBeNull();
        issuesListFromInlineAggregation.Should().NotBeNull();

        issuesListFromLiveAggregation!.Id.Should().Be(reservation.Id);
        issuesListFromInlineAggregation!.Id.Should().Be(reservation.Id);
    }
}


