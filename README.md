[![Twitter Follow](https://img.shields.io/twitter/follow/oskar_at_net?style=social)](https://twitter.com/oskar_at_net) [![Join the chat at https://gitter.im/EventSourcing-NetCore/community](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/EventSourcing-NetCore/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) ![Github Actions](https://github.com/oskardudycz/EventSourcing.NetCore/actions/workflows/build.dotnet.yml/badge.svg?branch=main) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_net) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net) 

# EventSourcing .NET

Tutorial, practical samples and other resources about Event Sourcing in .NET. See also my similar repositories for [JVM](https://github.com/oskardudycz/EventSourcing.JVM) and [NodeJS](https://github.com/oskardudycz/EventSourcing.NodeJS).

- [EventSourcing .NET](#eventsourcing-net)
  - [1. Event Sourcing](#1-event-sourcing)
    - [1.1 What is Event Sourcing?](#11-what-is-event-sourcing)
    - [1.2 What is Event?](#12-what-is-event)
    - [1.3 What is Stream?](#13-what-is-stream)
    - [1.4 Event representation](#14-event-representation)
    - [1.5 Event Storage](#15-event-storage)
    - [1.6 Retrieving the current state from events](#16-retrieving-the-current-state-from-events)
    - [1.7 Strongly-Typed ids with Marten](#17-strongly-typed-ids-with-marten)
  - [2. Videos](#2-videos)
    - [2.1. Practical Event Sourcing with Marten](#21-practical-event-sourcing-with-marten)
    - [2.2. Practical Introduction to Event Sourcing with EventStoreDB](#22-practical-introduction-to-event-sourcing-with-eventstoredb)
    - [2.3 Let's build the worst Event Sourcing system!](#23-lets-build-the-worst-event-sourcing-system)
    - [2.4 The Light and The Dark Side of the Event-Driven Design](#24-the-light-and-the-dark-side-of-the-event-driven-design)
    - [2.5 Conversation with Yves Lorphelin about CQRS](#25-conversation-with-yves-lorphelin-about-cqrs)
    - [2.6. CQRS is Simpler than you think with C#9 & NET5](#26-cqrs-is-simpler-than-you-think-with-c9--net5)
    - [2.7. Never Lose Data Again - Event Sourcing to the Rescue!](#27-never-lose-data-again---event-sourcing-to-the-rescue)
    - [2.8. How to deal with privacy and GDPR in Event-Sourced systems](#28-how-to-deal-with-privacy-and-gdpr-in-event-sourced-systems)
  - [3. Support](#3-support)
  - [4. Prerequisites](#4-prerequisites)
  - [5. Tools used](#5-tools-used)
  - [6. Samples](#6-samples)
    - [6.1 Pragmatic Event Sourcing With Marten](#61-pragmatic-event-sourcing-with-marten)
    - [6.2 ECommerce with Marten](#62-ecommerce-with-marten)
    - [6.3 Simple EventSourcing with EventStoreDB](#63-simple-eventsourcing-with-eventstoredb)
    - [6.4 ECommerce with EventStoreDB](#64-ecommerce-with-eventstoredb)
    - [6.5 Warehouse](#65-warehouse)
    - [6.6 Warehouse Minimal API](#66-warehouse-minimal-api)
    - [6.7 Event Versioning](#67-event-versioning)
    - [6.8 Event Pipelines](#68-event-pipelines)
    - [6.9 Meetings Management with Marten](#69-meetings-management-with-marten)
    - [6.10 Cinema Tickets Reservations with Marten](#610-cinema-tickets-reservations-with-marten)
    - [6.11 SmartHome IoT with Marten](#611-smarthome-iot-with-marten)
  - [7. Self-paced training Kits](#7-self-paced-training-kits)
    - [7.1 Introduction to Event Sourcing](#71-introduction-to-event-sourcing)
    - [7.2 Build your own Event Store](#72-build-your-own-event-store)
  - [8. Articles](#8-articles)
  - [9. Event Store - Marten](#9-event-store---marten)
  - [10. Message Bus (for processing Commands, Queries, Events) - MediatR](#10-message-bus-for-processing-commands-queries-events---mediatr)
  - [11. CQRS (Command Query Responsibility Separation)](#11-cqrs-command-query-responsibility-separation)
  - [12. NuGet packages to help you get started.](#12-nuget-packages-to-help-you-get-started)
  - [13. Other resources](#13-other-resources)
    - [13.1 Introduction](#131-introduction)
    - [13.2 Event Sourcing on production](#132-event-sourcing-on-production)
    - [13.3 Projections](#133-projections)
    - [13.4 Snapshots](#134-snapshots)
    - [13.5 Versioning](#135-versioning)
    - [13.6 Storage](#136-storage)
    - [13.7 Design & Modeling](#137-design--modeling)
    - [13.8 GDPR](#138-gdpr)
    - [13.9 Conflict Detection](#139-conflict-detection)
    - [13.10 Functional programming](#1310-functional-programming)
    - [13.12 Testing](#1312-testing)
    - [13.13 CQRS](#1313-cqrs)
    - [13.14 Tools](#1314-tools)
    - [13.15 Event processing](#1315-event-processing)
    - [13.16 Distributed processes](#1316-distributed-processes)
    - [13.17 Domain Driven Design](#1317-domain-driven-design)
    - [13.18 Whitepapers](#1318-whitepapers)
    - [13.19 This is NOT Event Sourcing (but Event Streaming)](#1319-this-is-not-event-sourcing-but-event-streaming)
    - [13.20 Event Sourcing Concerns](#1320-event-sourcing-concerns)
    - [13.21 Architecture Weekly](#1321-architecture-weekly)


## 1. Event Sourcing

### 1.1 What is Event Sourcing?

Event Sourcing is a design pattern in which results of business operations are stored as a series of events. 

It is an alternative way to persist data. In contrast with state-oriented persistence that only keeps the latest version of the entity state, Event Sourcing stores each state change as a separate event.

Thanks to that, no business data is lost. Each operation results in the event stored in the database. That enables extended auditing and diagnostics capabilities (both technically and business-wise). What's more, as events contains the business context, it allows wide business analysis and reporting.

In this repository I'm showing different aspects and patterns around Event Sourcing from the basic to advanced practices.

Read more in my articles:
-   📝 [How using events helps in a teams' autonomy](https://event-driven.io/en/how_using_events_help_in_teams_autonomy/?utm_source=event_sourcing_net)
-   📝 [When not to use Event Sourcing?](https://event-driven.io/en/when_not_to_use_event_sourcing/?utm_source=event_sourcing_net)

### 1.2 What is Event?

Events represent facts in the past. They carry information about something accomplished. It should be named in the past tense, e.g. _"user added"_, _"order confirmed"_. Events are not directed to a specific recipient - they're broadcasted information. It's like telling a story at a party. We hope that someone listens to us, but we may quickly realise that no one is paying attention.

Events:
- are immutable: _"What has been seen, cannot be unseen"_.
- can be ignored but cannot be retracted (as you cannot change the past).
- can be interpreted differently. The basketball match result is a fact. Winning team fans will interpret it positively. Losing team fans - not so much.

Read more in my articles:
-   📝 [What's the difference between a command and an event?](https://event-driven.io/en/whats_the_difference_between_event_and_command/?utm_source=event_sourcing_net)
-   📝 [Events should be as small as possible, right?](https://event-driven.io/en/events_should_be_as_small_as_possible/?utm_source=event_sourcing_net)
-   📝 [Anti-patterns in event modelling - Property Sourcing](https://event-driven.io/en/property-sourcing/?utm_source=event_sourcing_net)
-   📝 [Anti-patterns in event modelling - State Obsession](https://event-driven.io/en/state-obsession/?utm_source=event_sourcing_net)

### 1.3 What is Stream?

Events are logically grouped into streams. In Event Sourcing, streams are the representation of the entities. All the entity state mutations end up as the persisted events. Entity state is retrieved by reading all the stream events and applying them one by one in the order of appearance.

A stream should have a unique identifier representing the specific object. Each event has its own unique position within a stream. This position is usually represented by a numeric, incremental value. This number can be used to define the order of the events while retrieving the state. It can also be used to detect concurrency issues. 

### 1.4 Event representation

Technically events are messages. 

They may be represented, e.g. in JSON, Binary, XML format. Besides the data, they usually contain:
- **id**: unique event identifier.
- **type**: name of the event, e.g. _"invoice issued"_.
- **stream id**: object id for which event was registered (e.g. invoice id).
- **stream position** (also named _version_, _order of occurrence_, etc.): the number used to decide the order of the event's occurrence for the specific object (stream).
- **timestamp**: representing a time at which the event happened.
- other metadata like `correlation id`, `causation id`, etc.

Sample event JSON can look like:

```json
{
  "id": "e44f813c-1a2f-4747-aed5-086805c6450e",
  "type": "invoice-issued",
  "streamId": "INV/2021/11/01",
  "streamPosition": 1,
  "timestamp": "2021-11-01T00:05:32.000Z",

  "data":
  {
    "issuedTo": {
      "name": "Oscar the Grouch",
      "address": "123 Sesame Street"
    },
    "amount": 34.12,
    "number": "INV/2021/11/01",
    "issuedAt": "2021-11-01T00:05:32.000Z"
  },

  "metadata": 
  {
    "correlationId": "1fecc92e-3197-4191-b929-bd306e1110a4",
    "causationId": "c3cf07e8-9f2f-4c2d-a8e9-f8a612b4a7f1"
  }
}
```
### 1.5 Event Storage

Event Sourcing is not related to any type of storage implementation. As long as it fulfills the assumptions, it can be implemented having any backing database (relational, document, etc.). The state has to be represented by the append-only log of events. The events are stored in chronological order, and new events are appended to the previous event. Event Stores are the databases' category explicitly designed for such purpose. 

Read more in my article:
-   📝 [What if I told you that Relational Databases are in fact Event Stores?](https://event-driven.io/en/relational_databases_are_event_stores/?utm_source=event_sourcing_net)

### 1.6 Retrieving the current state from events

In Event Sourcing, the state is stored in events. Events are logically grouped into streams. Streams can be thought of as the entities' representation. Traditionally (e.g. in relational or document approach), each entity is stored as a separate record.

| Id       | IssuerName       | IssuerAddress     | Amount | Number         | IssuedAt   |
| -------- | ---------------- | ----------------- | ------ | -------------- | ---------- |
| e44f813c | Oscar the Grouch | 123 Sesame Street | 34.12  | INV/2021/11/01 | 2021-11-01 |

 In Event Sourcing, the entity is stored as the series of events that happened for this specific object, e.g. `InvoiceInitiated`, `InvoiceIssued`, `InvoiceSent`. 

```json          
[
    {
        "id": "e44f813c-1a2f-4747-aed5-086805c6450e",
        "type": "invoice-initiated",
        "streamId": "INV/2021/11/01",
        "streamPosition": 1,
        "timestamp": "2021-11-01T00:05:32.000Z",

        "data":
        {
            "issuer": {
                "name": "Oscar the Grouch",
                "address": "123 Sesame Street",
            },
            "amount": 34.12,
            "number": "INV/2021/11/01",
            "initiatedAt": "2021-11-01T00:05:32.000Z"
        }
    },        
    {
        "id": "5421d67d-d0fe-4c4c-b232-ff284810fb59",
        "type": "invoice-issued",
        "streamId": "INV/2021/11/01",
        "streamPosition": 2,
        "timestamp": "2021-11-01T00:11:32.000Z",

        "data":
        {
            "issuedTo": "Cookie Monster",
            "issuedAt": "2021-11-01T00:11:32.000Z"
        }
    },        
    {
        "id": "637cfe0f-ed38-4595-8b17-2534cc706abf",
        "type": "invoice-sent",
        "streamId": "INV/2021/11/01",
        "streamPosition": 3,
        "timestamp": "2021-11-01T00:12:01.000Z",

        "data":
        {
            "sentVia": "email",
            "sentAt": "2021-11-01T00:12:01.000Z"
        }
    }
]
```

All of those events share the stream id (`"streamId": "INV/2021/11/01"`), and have incremented stream positions.

In Event Sourcing each entity is represented by its stream: the sequence of events correlated by the stream id ordered by stream position.

To get the current state of an entity we need to perform the stream aggregation process. We're translating the set of events into a single entity. This can be done with the following steps:
1. Read all events for the specific stream.
2. Order them ascending in the order of appearance (by the event's stream position).
3. Construct the empty object of the entity type (e.g. with default constructor).
4. Apply each event on the entity.

This process is called also _stream aggregation_ or _state rehydration_.

We could implement that as:

```csharp
public record Person(
    string Name,
    string Address
);

public record InvoiceInitiated(
    double Amount,
    string Number,
    Person IssuedTo,
    DateTime InitiatedAt
);

public record InvoiceIssued(
    string IssuedBy,
    DateTime IssuedAt
);

public enum InvoiceSendMethod
{
    Email,
    Post
}

public record InvoiceSent(
    InvoiceSendMethod SentVia,
    DateTime SentAt
);

public enum InvoiceStatus
{
    Initiated = 1,
    Issued = 2,
    Sent = 3
}

public class Invoice
{
    public string Id { get;set; }
    public double Amount { get; private set; }
    public string Number { get; private set; }

    public InvoiceStatus Status { get; private set; }

    public Person IssuedTo { get; private set; }
    public DateTime InitiatedAt { get; private set; }

    public string IssuedBy { get; private set; }
    public DateTime IssuedAt { get; private set; }

    public InvoiceSendMethod SentVia { get; private set; }
    public DateTime SentAt { get; private set; }

    public void When(object @event)
    {
        switch (@event)
        {
            case InvoiceInitiated invoiceInitiated:
                Apply(invoiceInitiated);
                break;
            case InvoiceIssued invoiceIssued:
                Apply(invoiceIssued);
                break;
            case InvoiceSent invoiceSent:
                Apply(invoiceSent);
                break;
        }
    }

    private void Apply(InvoiceInitiated @event)
    {
        Id = @event.Number;
        Amount = @event.Amount;
        Number = @event.Number;
        IssuedTo = @event.IssuedTo;
        InitiatedAt = @event.InitiatedAt;
        Status = InvoiceStatus.Initiated;
    }

    private void Apply(InvoiceIssued @event)
    {
        IssuedBy = @event.IssuedBy;
        IssuedAt = @event.IssuedAt;
        Status = InvoiceStatus.Issued;
    }

    private void Apply(InvoiceSent @event)
    {
        SentVia = @event.SentVia;
        SentAt = @event.SentAt;
        Status = InvoiceStatus.Sent;
    }
}
```
and use it as:

```csharp
var invoiceInitiated = new InvoiceInitiated(
    34.12,
    "INV/2021/11/01",
    new Person("Oscar the Grouch", "123 Sesame Street"),
    DateTime.UtcNow
);
var invoiceIssued = new InvoiceIssued(
    "Cookie Monster",
    DateTime.UtcNow
);
var invoiceSent = new InvoiceSent(
    InvoiceSendMethod.Email,
    DateTime.UtcNow
);

// 1,2. Get all events and sort them in the order of appearance
var events = new object[] {invoiceInitiated, invoiceIssued, invoiceSent};

// 3. Construct empty Invoice object
var invoice = new Invoice();

// 4. Apply each event on the entity.
foreach (var @event in events)
{
    invoice.When(@event);
}
```

and generalise this into `Aggregate` base class:

```csharp
public abstract class Aggregate<T>
{
    public T Id { get; protected set; }
    
    protected Aggregate() { }
        
    public virtual void When(object @event) { }
}
```

The biggest advantage of _"online"_ stream aggregation is that it always uses the most recent business logic. So after the change in the apply method, it's automatically reflected on the next run. If events data is fine, then it's not needed to do any migration or updates.

In Marten `When` method is not needed. Marten uses naming convention and call the `Apply` method internally. It has to:
- have single parameter with event object,
- have `void` type as the result.

See samples:
- [Generic stream aggregation](/Core.Tests/AggregateWithWhenTests.cs)
- [Marten](/Marten.Integration.Tests/EventStore/Aggregate/EventsAggregation.cs) 
- [EventStoreDB](/Core.EventStoreDB/Events/AggregateStreamExtensions.cs) 


Read more in my article:
-   📝 [How to get the current entity state from events?](https://event-driven.io/en/how_to_get_the_current_entity_state_in_event_sourcing/?utm_source=event_sourcing_net)

### 1.7 Strongly-Typed ids with Marten

Strongly typed ids (or, in general, a proper type system) can make your code more predictable. It reduces the chance of trivial mistakes, like accidentally changing parameters order of the same primitive type. 

So for such code:

```csharp
var reservationId = "RES/01";
var seatId = "SEAT/22";
var customerId = "CUS/291";

var reservation = new ReservationId (
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

var reservation = new ReservationId (
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

See the full sample [here](./Marten.Integration.Tests/CompositeIds/CompositeIdsTests.cs).

Read more in the article:
-   📝 [Using strongly-typed identifiers with Marten](https://event-driven.io/en/using_strongly_typed_ids_with_marten//?utm_source=event_sourcing_net)
-   📝 [Immutable Value Objects are simpler and more useful than you think!](https://event-driven.io/en/immutable_value_objects/?utm_source=event_sourcing_net)

## 2. Videos

### 2.1. Practical Event Sourcing with Marten

<a href="https://www.youtube.com/watch?v=L_ized5xwww&list=PLw-VZz_H4iio9b_NrH25gPKjr2MAS2YgC&index=7" target="_blank"><img src="https://img.youtube.com/vi/L_ized5xwww/0.jpg" alt="Practical Event Sourcing with Marten (EN)" width="320" height="240" border="10" /></a>

### 2.2. Practical Introduction to Event Sourcing with EventStoreDB

<a href="https://www.youtube.com/watch?v=rqYPVzjoxqI" target="_blank"><img src="https://img.youtube.com/vi/rqYPVzjoxqI/0.jpg" alt="Practical introduction to Event Sourcing with EventStoreDB" width="320" height="240" border="10" /></a>

### 2.3 Let's build the worst Event Sourcing system!

<a href="https://www.youtube.com/watch?v=Lu-skMQ-vAw" target="_blank"><img src="https://img.youtube.com/vi/Lu-skMQ-vAw/0.jpg" alt="Let's build the worst Event Sourcing system!" width="320" height="240" border="10" /></a>

### 2.4 The Light and The Dark Side of the Event-Driven Design

<a href="https://www.youtube.com/watch?v=0pYmuk0-N_4" target="_blank"><img src="https://img.youtube.com/vi/0pYmuk0-N_4/0.jpg" alt="The Light and The Dark Side of the Event-Driven Design" width="320" height="240" border="10" /></a>

### 2.5 Conversation with [Yves Lorphelin](https://github.com/ylorph/) about CQRS

<a href="https://www.youtube.com/watch?v=D-3N2vQ7ADE" target="_blank"><img src="https://img.youtube.com/vi/D-3N2vQ7ADE/0.jpg" alt="Event Store Conversations: Yves Lorphelin talks to Oskar Dudycz about CQRS (EN)" width="320" height="240" border="10" /></a>

### 2.6. CQRS is Simpler than you think with C#9 & NET5

<a href="https://www.youtube.com/watch?v=eOPlg-eB4As" target="_blank"><img src="https://img.youtube.com/vi/eOPlg-eB4As/0.jpg" alt="CQRS is Simpler than you think with C#9 & NET5" width="320" height="240" border="10" /></a>

### 2.7. Never Lose Data Again - Event Sourcing to the Rescue!

<a href="https://www.youtube.com/watch?v=fDC465jJoDk" target="_blank"><img src="https://img.youtube.com/vi/fDC465jJoDk/0.jpg" alt="Never Lose Data Again - Event Sourcing to the Rescue!" width="320" height="240" border="10" /></a>

### 2.8. How to deal with privacy and GDPR in Event-Sourced systems

<a href="https://www.youtube.com/watch?v=CI7JPFLlpBw" target="_blank"><img src="https://img.youtube.com/vi/CI7JPFLlpBw/0.jpg" alt="How to deal with privacy and GDPR in Event-Sourced systems" width="320" height="240" border="10" /></a>

## 3. Support

Feel free to [create an issue](https://github.com/oskardudycz/EventSourcing.NetCore/issues/new) if you have any questions or request for more explanation or samples. I also take **Pull Requests**!

💖 If this repository helped you - I'd be more than happy if you **join** the group of **my official supporters** at:

👉 [Github Sponsors](https://github.com/sponsors/oskardudycz) 

⭐ Star on GitHub or sharing with your friends will also help!

## 4. Prerequisites

For running the Event Store examples you need to have:

1. .NET 6 installed - https://dotnet.microsoft.com/download/dotnet/6.0
2. [Docker](https://store.docker.com/search?type=edition&offering=community) installed. Then going to the `docker` folder and running:

```
docker-compose up
```

**More information about using .NET, WebApi and Docker you can find in my other tutorials:** [WebApi with .NET](https://github.com/oskardudycz/WebApiWith.NETCore)

## 5. Tools used

1. [Marten](https://martendb.io/) - Event Store and Read Models
2. [EventStoreDB](https://eventstore.com) - Event Store 
3. [MediatR](https://github.com/jbogard/MediatR) - Internal In-Memory Message Bus (for processing Commands, Queries, Events)
4. [Kafka](https://kafka.apache.org/) - External Durable Message Bus to integrate services
5. [ElasticSearch](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/nest-getting-started.html) - Read Models

## 6. Samples

See also fully working, real-world samples of Event Sourcing and CQRS applications in [Samples folder](https://github.com/oskardudycz/EventSourcing.NetCore/tree/main/Sample).

Samples are using CQRS architecture. They're sliced based on the business modules and operations. Read more about the assumptions in ["How to slice the codebase effectively?"](https://event-driven.io/en/how_to_slice_the_codebase_effectively/?utm_source=event_sourcing_net).

### 6.1 [Pragmatic Event Sourcing With Marten](./Sample/Helpdesk)
- Simplest CQRS and Event Sourcing flow using Minimal API,
- Cutting the number of layers and boilerplate complex code to bare minimum,
- Using all Marten helpers like `WriteToAggregate`, `AggregateStream` to simplify the processing,
- Examples of all the typical Marten's projections,
- Example of how and where to use C# Records, Nullable Reference Types, etc,
- No Aggregates. Commands are handled in the domain service as pure functions.

### 6.2 [ECommerce with Marten](./Sample/ECommerce)
- typical Event Sourcing and CQRS flow,
- DDD using Aggregates,
- microservices example,
- stores events to Marten,
- distributed processes coordinated by Saga ([Order Saga](./Sample/ECommerce/Orders/Orders/Orders/OrderSaga.cs)),
- Kafka as a messaging platform to integrate microservices,
- example of the case when some services are event-sourced ([Carts](./Sample/ECommerce/Carts), [Orders](./Sample/ECommerce/Orders), [Payments](./Sample/ECommerce/Payments)) and some are not ([Shipments](./Sample/ECommerce/Shipments) using EntityFramework as ORM)

### 6.3 [Simple EventSourcing with EventStoreDB](./Sample/EventStoreDB/Simple)
- typical Event Sourcing and CQRS flow,
- functional composition, no aggregates, just data and functions,
- stores events to  EventStoreDB,
- Builds read models using [Subscription to `$all`](https://developers.eventstore.com/clients/grpc/subscribing-to-streams/#subscribing-to-all),
- Read models are stored as Postgres tables using EntityFramework.

### 6.4 [ECommerce with EventStoreDB](./Sample/EventStoreDB/ECommerce) 
- typical Event Sourcing and CQRS flow,
- DDD using Aggregates,
- stores events to  EventStoreDB,
- Builds read models using [Subscription to `$all`](https://developers.eventstore.com/clients/grpc/subscribing-to-streams/#subscribing-to-all).
- Read models are stored as Marten documents.

### 6.5 [Warehouse](./Sample/Warehouse)
- simplest CQRS flow using .NET Endpoints,
- example of how and where to use C# Records, Nullable Reference Types, etc,
- No Event Sourcing! Using Entity Framework to show that CQRS is not bounded to Event Sourcing or any type of storage,
- No Aggregates! CQRS do not need DDD. Business logic can be handled in handlers.

### 6.6 [Warehouse Minimal API](./Sample/Warehouse.MinimalAPI/)
Variation of the previous example, but:
- using Minimal API,
- example how to inject handlers in MediatR like style to decouple API from handlers.
- 📝 Read more [CQRS is simpler than you think with .NET 6 and C# 10](https://event-driven.io/en/cqrs_is_simpler_than_you_think_with_net6/?utm_source=event_sourcing_net) 

### 6.7 [Event Versioning](./Sample/EventsVersioning)
Shows how to handle basic event schema versioning scenarios using event and stream transformations (e.g. upcasting):
- [Simple mapping](./Sample/EventsVersioning/#simple-mapping)
  - [New not required property](./Sample/EventsVersioning/#new-not-required-property)
  - [New required property](./Sample/EventsVersioning/#new-required-property)
  - [Renamed property](./Sample/EventsVersioning/#renamed-property)
- [Upcasting](./Sample/EventsVersioning/#upcasting)
  - [Changed Structure](./Sample/EventsVersioning/#changed-structure)
  - [New required property](./Sample/EventsVersioning/#new-required-property-1)
- [Downcasters](./Sample/EventsVersioning/#downcasters)
- [Events Transformations](./Sample/EventsVersioning/#events-transformations)
- [Stream Transformation](./Sample/EventsVersioning/#stream-transformation)
- [Summary](./Sample/EventsVersioning/#summary)
- 📝 [Simple patterns for events schema versioning](https://event-driven.io/en/simple_events_versioning_patterns/?utm_source=event_sourcing_net) 

### 6.8 [Event Pipelines](./Sample/EventPipelines)
Shows how to compose event handlers in the processing pipelines to:
- filter events,
- transform them,
- NOT requiring marker interfaces for events,
- NOT requiring marker interfaces for handlers,
- enables composition through regular functions,
- allows using interfaces and classes if you want to,
- can be used with Dependency Injection, but also without through builder,
- integrates with MediatR if you want to.
- 📝 Read more [How to build a simple event pipeline](https://event-driven.io/en/how_to_build_simple_event_pipeline/?utm_source=event_sourcing_net) 

### 6.9 [Meetings Management with Marten](./Sample/MeetingsManagement/)
- typical Event Sourcing and CQRS flow,
- DDD using Aggregates,
- microservices example,
- stores events to Marten,
- Kafka as a messaging platform to integrate microservices,
- read models handled in separate microservice and stored to other database (ElasticSearch)

### 6.10 [Cinema Tickets Reservations with Marten](./Sample/Tickets/)
- typical Event Sourcing and CQRS flow,
- DDD using Aggregates,
- stores events to Marten.

### 6.11 [SmartHome IoT with Marten](./Sample/AsyncProjections/)
- typical Event Sourcing and CQRS flow,
- DDD using Aggregates,
- stores events to Marten,
- asynchronous projections rebuild using AsynDaemon feature.

## 7. Self-paced training Kits

I prepared the self-paced training Kits for the Event Sourcing. See more in the [Workshop description](./Workshops/BuildYourOwnEventStore/Readme.md).

### 7.1 [Introduction to Event Sourcing](./Workshops/IntroductionToEventSourcing)

Event Sourcing is perceived as a complex pattern. Some believe that it's like Nessie, everyone's heard about it, but rarely seen it. In fact, Event Sourcing is a pretty practical and straightforward concept. It helps build predictable applications closer to business. Nowadays, storage is cheap, and information is priceless. In Event Sourcing, no data is lost. 

The workshop aims to build the knowledge of the general concept and its related patterns for the participants. The acquired knowledge will allow for the conscious design of architectural solutions and the analysis of associated risks. 

The emphasis will be on a pragmatic understanding of architectures and applying it in practice using Marten and EventStoreDB.

You can do the workshop as a self-paced kit. That should give you a good foundation for starting your journey with Event Sourcing and learning tools like Marten and EventStoreDB. If you'd like to get full coverage with all nuances of the private workshop, feel free to contact me via [email](mailto:oskar.dudycz@gmail.com).

1. [Events definition](./Workshops/IntroductionToEventSourcing/01-EventsDefinition).
2. [Getting State from events](./Workshops/IntroductionToEventSourcing/02-GettingStateFromEvents).
3. Appending Events:
   * [Marten](./Workshops/IntroductionToEventSourcing/03-AppendingEvents.Marten)
   * [EventStoreDB](./Workshops/IntroductionToEventSourcing/04-AppendingEvents.EventStoreDB)
4. Getting State from events
   * [Marten](./Workshops/IntroductionToEventSourcing/05-GettingStateFromEvents.Marten)
   * [EventStoreDB](./Workshops/IntroductionToEventSourcing/06-GettingStateFromEvents.EventStoreDB)
5. Business logic:
   * [General](./Workshops/IntroductionToEventSourcing/07-BusinessLogic)
   * [Marten](./Workshops/IntroductionToEventSourcing/08-BusinessLogic.Marten)
   * [EventStoreDB](./Workshops/IntroductionToEventSourcing/09-BusinessLogic.EventStoreDB)
6. Optimistic Concurrency:
   * [Marten](./Workshops/IntroductionToEventSourcing/10-OptimisticConcurrency.Marten)
   * [EventStoreDB](./Workshops/IntroductionToEventSourcing/11-OptimisticConcurrency.EventStoreDB)
7. Projections:
   * [General](./Workshops/IntroductionToEventSourcing/12-Projections)
   * [Idempotency](./Workshops/IntroductionToEventSourcing/13-Projections.Idempotency)
   * [Eventual Consistency](./Workshops/IntroductionToEventSourcing/14-Projections.EventualConsistency)

### 7.2 [Build your own Event Store](./Workshops/BuildYourOwnEventStore)

It teaches the event store basics by showing how to build your Event Store on top of Relational Database. It starts with the tables setup, goes through appending events, aggregations, projections, snapshots, and finishes with the `Marten` basics.

1. [Streams Table](./Workshops/BuildYourOwnEventStore/01-CreateStreamsTable)
2. [Events Table](./Workshops/BuildYourOwnEventStore/02-CreateEventsTable)
3. [Appending Events](./Workshops/BuildYourOwnEventStore/03-CreateAppendEventFunction)
4. [Optimistic Concurrency Handling](./Workshops/BuildYourOwnEventStore/03-OptimisticConcurrency)
5. [Event Store Methods](./Workshops/BuildYourOwnEventStore/04-EventStoreMethods)
6. [Stream Aggregation](./Workshops/BuildYourOwnEventStore/05-StreamAggregation)
7. [Time Travelling](./Workshops/BuildYourOwnEventStore/06-TimeTraveling)
8. [Aggregate and Repositories](./Workshops/BuildYourOwnEventStore/07-AggregateAndRepository)
9. [Snapshots](./Workshops/BuildYourOwnEventStore/08-Snapshots)
10. [Projections](./Workshops/BuildYourOwnEventStore/09-Projections)
11. [Projections With Marten](./Workshops/BuildYourOwnEventStore/10-ProjectionsWithMarten)

## 8. Articles
Read also more on the **Event Sourcing** and **CQRS** topics in my [blog](https://event-driven.io/?utm_source=event_sourcing_net) posts:
-   📝 [Introduction to Event Sourcing - Self Paced Kit](https://event-driven.io/en/introduction_to_event_sourcing/?utm_source=event_sourcing_net)
-   📝 [What's the difference between a command and an event?](https://event-driven.io/en/whats_the_difference_between_event_and_command/?utm_source=event_sourcing_net)
-   📝 [Event Streaming is not Event Sourcing!](https://event-driven.io/en/event_streaming_is_not_event_sourcing/?utm_source=event_sourcing_net)
-   📝 [Events should be as small as possible, right?](https://event-driven.io/en/events_should_be_as_small_as_possible/?utm_source=event_sourcing_net)
-   📝 [How to get the current entity state from events?](https://event-driven.io/en/how_to_get_the_current_entity_state_in_event_sourcing/?utm_source=event_sourcing_net)
-   📝 [How to ensure uniqueness in Event Sourcing](https://event-driven.io/en/uniqueness-in-event-sourcing/?utm_source=event_sourcing_net)
-   📝 [Anti-patterns in event modelling - Property Sourcing](https://event-driven.io/en/property-sourcing/?utm_source=event_sourcing_net)
-   📝 [Anti-patterns in event modelling - State Obsession](https://event-driven.io/en/state-obsession/?utm_source=event_sourcing_net)
-   📝 [Why a bank account is not the best example of Event Sourcing?](https://event-driven.io/en/bank_account_event_sourcing/?utm_source=event_sourcing_net)
-   📝 [When not to use Event Sourcing?](https://event-driven.io/en/when_not_to_use_event_sourcing/?utm_source=event_sourcing_net)
-   📝 [CQRS facts and myths explained](https://event-driven.io/en/cqrs_facts_and_myths_explained/?utm_source=event_sourcing_net)
-   📝 [How to slice the codebase effectively?](https://event-driven.io/en/how_to_slice_the_codebase_effectively/?utm_source=event_sourcing_net)
-   📝 [Generic does not mean Simple?](https://event-driven.io/en/generic_does_not_mean_simple/?utm_source=event_sourcing_net)
-   📝 [Can command return a value?](https://event-driven.io/en/can_command_return_a_value/?utm_source=event_sourcing_net)
-   📝 [CQRS is simpler than you think with .NET 6 and C# 10](https://event-driven.io/en/cqrs_is_simpler_than_you_think_with_net6/?utm_source=event_sourcing_net)   
-   📝 [How to register all CQRS handlers by convention](https://event-driven.io/en/how_to_register_all_mediatr_handlers_by_convention/?utm_source=event_sourcing_net)
-   📝 [How to use ETag header for optimistic concurrency](https://event-driven.io/en/how_to_use_etag_header_for_optimistic_concurrency/?utm_source=event_sourcing_net)
-   📝 [Dealing with Eventual Consistency and Idempotency in MongoDB projections](https://event-driven.io/en/dealing_with_eventual_consistency_and_idempotency_in_mongodb_projections/?utm_source=event_sourcing_net)
-   📝 [Long-polling, how to make our async API synchronous](https://event-driven.io/en/long_polling_and_eventual_consistency/?utm_source=event_sourcing_net)
-   📝 [A simple trick for idempotency handling in the Elastic Search read model](https://event-driven.io/en/simple_trick_for_idempotency_handling_in_elastic_search_readm_model/?utm_source=event_sourcing_net)
-   📝 [How to do snapshots in Marten?](https://event-driven.io/en/how_to_do_snapshots_in_Marten/?utm_source=event_sourcing_net)
-   📝 [Integrating Marten with other systems](https://event-driven.io/en/integrating_Marten/?utm_source=event_sourcing_net)
-   📝 [How to (not) do the events versioning?](https://event-driven.io/en/how_to_do_event_versioning/?utm_source=event_sourcing_net)
-   📝 [Simple patterns for events schema versioning](https://event-driven.io/en/simple_events_versioning_patterns/?utm_source=event_sourcing_net) 
-   📝 [How to build a simple event pipeline](https://event-driven.io/en/how_to_build_simple_event_pipeline/?utm_source=event_sourcing_net) 
-   📝 [How to create projections of events for nested object structures?](https://event-driven.io/en/how_to_create_projections_of_events_for_nested_object_structures/?utm_source=event_sourcing_net)
-   📝 [How to scale projections in the event-driven systems?](https://event-driven.io/en/how_to_scale_projections_in_the_event_driven_systems/?utm_source=event_sourcing_net)
-   📝 [Immutable Value Objects are simpler and more useful than you think!](https://event-driven.io/en/immutable_value_objects/?utm_source=event_sourcing_net)
-   📝 [Notes about C# records and Nullable Reference Types](https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/?utm_source=event_sourcing_net)
-   📝 [Using strongly-typed identifiers with Marten](https://event-driven.io/en/using_strongly_typed_ids_with_marten/?utm_source=event_sourcing_net)
-   📝 [How using events helps in a teams' autonomy](https://event-driven.io/en/how_using_events_help_in_teams_autonomy/?utm_source=event_sourcing_net)
-   📝 [What texting your Ex has to do with Event-Driven Design?](https://event-driven.io/en/what_texting_ex_has_to_do_with_event_driven_design/?utm_source=event_sourcing_net)
-   📝 [What if I told you that Relational Databases are in fact Event Stores?](https://event-driven.io/en/relational_databases_are_event_stores/?utm_source=event_sourcing_net)
-   📝 [Optimistic concurrency for pessimistic times](https://event-driven.io/en/optimistic_concurrency_for_pessimistic_times/?utm_source=event_sourcing_net)
-   📝 [Outbox, Inbox patterns and delivery guarantees explained](https://event-driven.io/en/outbox_inbox_patterns_and_delivery_guarantees_explained/?utm_source=event_sourcing_net)
-   📝 [Saga and Process Manager - distributed processes in practice](https://event-driven.io/en/saga_process_manager_distributed_transactions/?utm_source=event_sourcing_net)

## 9. Event Store - Marten

-   **[Creating event store](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Marten.Integration.Tests/General/StoreInitializationTests.cs)**
-   **Event Stream** - is a representation of the entity in event sourcing. It's a set of events that happened for the entity with the exact id. Stream id should be unique, can have different types but usually is a Guid.
    -   **[Stream starting](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Marten.Integration.Tests/EventStore/Stream/StreamStarting.cs)** - stream should be always started with a unique id. Marten provides three ways of starting the stream:
        -   calling StartStream method with a stream id
            ```csharp
            var streamId = Guid.NewGuid();
            documentSession.Events.StartStream<IssuesList>(streamId);
            ```
        -   calling StartStream method with a set of events
            ```csharp
            var @event = new IssueCreated { IssueId = Guid.NewGuid(), Description = "Description" };
            var streamId = documentSession.Events.StartStream<IssuesList>(@event);
            ```
        -   just appending events with a stream id
            ```csharp
            var @event = new IssueCreated { IssueId = Guid.NewGuid(), Description = "Description" };
            var streamId = Guid.NewGuid();
            documentSession.Events.Append(streamId, @event);
            ```
    -   **[Stream loading](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Marten.Integration.Tests/EventStore/Stream/StreamLoading.cs)** - all events that were placed on the event store should be possible to load them back. [Marten](https://github.com/JasperFx/marten) allows to:
        -   get list of event by calling FetchStream method with a stream id
            ```csharp
            var eventsList = documentSession.Events.FetchStream(streamId);
            ```
        -   geting one event by its id
            ```csharp
            var @event = documentSession.Events.Load<IssueCreated>(eventId);
            ```
    -   **[Stream loading from exact state](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Marten.Integration.Tests/EventStore/Stream/StreamLoadingFromExactState.cs)** - all events that were placed on the event store should be possible to load them back. Marten allows to get stream from exact state by:
        -   timestamp (has to be in UTC)
            ```csharp
            var dateTime = new DateTime(2017, 1, 11);
            var events = documentSession.Events.FetchStream(streamId, timestamp: dateTime);
            ```
        -   version number
            ```csharp
            var versionNumber = 3;
            var events = documentSession.Events.FetchStream(streamId, version: versionNumber);
            ```
-   **Event stream aggregation** - events that were stored can be aggregated to form the entity once again. During the aggregation, process events are taken by the stream id and then replied event by event (so eg. NewTaskAdded, DescriptionOfTaskChanged, TaskRemoved). At first, an empty entity instance is being created (by calling default constructor). Then events based on the order of appearance are being applied on the entity instance by calling proper Apply methods.
    -   **[Online Aggregation](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Marten.Integration.Tests/EventStore/Aggregate/EventsAggregation.cs)** - online aggregation is a process when entity instance is being constructed on the fly from events. Events are taken from the database and then aggregation is being done. The biggest advantage of online aggregation is that it always gets the most recent business logic. So after the change, it's automatically reflected and it's not needed to do any migration or updates.
    -   **[Inline Aggregation (Snapshot)](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Marten.Integration.Tests/EventStore/Aggregate/InlineAggregationStorage.cs)** - inline aggregation happens when we take the snapshot of the entity from the DB. In that case, it's not needed to get all events. Marten stores the snapshot as a document. This is good for performance reasons because only one record is being materialized. The con of using inline aggregation is that after business logic has changed records need to be reaggregated.
    -   **[Reaggregation](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Marten.Integration.Tests/EventStore/Aggregate/Reaggregation.cs)** - one of the biggest advantages of the event sourcing is flexibility to business logic updates. It's not needed to perform complex migration. For online aggregation it's not needed to perform reaggregation - it's being made always automatically. The inline aggregation needs to be reaggregated. It can be done by performing online aggregation on all stream events and storing the result as a snapshot.
        -   reaggregation of inline snapshot with Marten
            ```csharp
            var onlineAggregation = documentSession.Events.AggregateStream<TEntity>(streamId);
            documentSession.Store<TEntity>(onlineAggregation);
            documentSession.SaveChanges();
            ```
-   **Event transformations**
    -   **[One event to one object transformations](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Marten.Integration.Tests/EventStore/Transformations/OneToOneEventTransformations.cs)**
    -   **[Inline Transformation storage](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Marten.Integration.Tests/EventStore/Transformations/InlineTransformationsStorage.cs)**
-   **Events projection**
    -   **[Projection of single stream](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Marten.Integration.Tests/EventStore/Projections/AggregationProjectionsTest.cs)**
-   **[Multitenancy per schema](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Marten.Integration.Tests/Tenancy/TenancyPerSchema.cs)**

## 10. Message Bus (for processing Commands, Queries, Events) - MediatR

-   **[Initialization](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/MediatR.Tests/Initialization/Initialization.cs)** - MediatR uses services locator pattern to find a proper handler for the message type.
-   **Sending Messages** - finds and uses the first registered handler for the message type. It could be used for queries (when we need to return values), commands (when we acting).
    -   **[No Handlers](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/MediatR.Tests/Sending/NoHandlers.cs)** - when MediatR doesn't find proper handler it throws an exception.
    -   **[Single Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/MediatR.Tests/Sending/SingleHandler.cs)** - by implementing IRequestHandler we're deciding that this handler should be run asynchronously with other async handlers (so we don't wait for the previous handler to finish its work).
    -   **[More Than One Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/MediatR.Tests/Sending/MoreThanOneHandler.cs)** - when there is more than one handler registered MediatR takes only one ignoring others when Send method is being called.
-   **Publishing Messages** - finds and uses all registered handlers for the message type. It's good for processing events.
    -   **[No Handlers](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/MediatR.Tests/Publishing/NoHandlers.cs)** - when MediatR doesn't find proper handler it throws an exception
    -   **[Single Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/MediatR.Tests/Publishing/SingleHandler.cs)** - by implementing INotificationHandler we're deciding that this handler should be run asynchronously with other async handlers (so we don't wait for the previous handler to finish its work)
    -   **[More Than One Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/MediatR.Tests/Publishing/MoreThanOneHandler.cs)** - when there is more than one handler registered MediatR takes all of them when calling Publish method
-   Pipeline (to be defined)

## 11. CQRS (Command Query Responsibility Separation)

-   **[Command handling](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/CQRS.Tests/Commands/Commands.cs)**
-   **[Query handling](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/CQRS.Tests/Queries/Queries.cs)**

## 12. NuGet packages to help you get started.

I gathered and generalized all of the practices used in this tutorial/samples in Nuget Packages maintained by me [GoldenEye Framework](https://github.com/oskardudycz/GoldenEye).
See more in:

-   [GoldenEye DDD package](https://github.com/oskardudycz/GoldenEye/tree/main/src/Core/Backend.Core.DDD) - it provides a set of base and bootstrap classes that helps you to reduce boilerplate code and help you focus on writing business code. You can find all classes like Commands/Queries/Event handlers and many more. To use it run:

    `dotnet add package GoldenEye.Backend.Core.DDD`

-   [GoldenEye Marten package](https://github.com/oskardudycz/GoldenEye/tree/main/src/Core/Backend.Core.Marten) - contains helpers, and abstractions to use Marten as document/event store. Gives you abstractions like repositories etc. To use it run:

    `dotnet add package GoldenEye.Backend.Core.Marten`

The simplest way to start is **installing the [project template](https://github.com/oskardudycz/GoldenEye/tree/main/src/Templates/SimpleDDD/content) by running**

`dotnet new -i GoldenEye.WebApi.Template.SimpleDDD`

**and then creating a new project based on it:**

`dotnet new SimpleDDD -n NameOfYourProject`

## 13. Other resources

### 13.1 Introduction
-   📝 [Event Store - A Beginner's Guide to Event Sourcing](https://www.eventstore.com/event-sourcing)
-   🎞 [Greg Young - CQRS & Event Sourcing](https://youtube.com/watch?v=JHGkaShoyNs)
-   📰 [Lorenzo Nicora - A visual introduction to event sourcing and cqrs](https://www.slideshare.net/LorenzoNicora/a-visual-introduction-to-event-sourcing-and-cqrs)
-   🎞 [Mathew McLoughlin - An Introduction to CQRS and Event Sourcing Patterns](https://www.youtube.com/watch?v=9a1PqwFrMP0)
-   🎞 [Emily Stamey - Hey Boss, Event Sourcing Could Fix That!](https://www.youtube.com/watch?v=mw7D6OJpsIA)
-   🎞 [Derek Comartin - Event Sourcing Example & Explained in plain English](https://www.youtube.com/watch?v=AUj4M-st3ic)
-   🎞 [Duncan Jones - Introduction to event sourcing and CQRS](https://www.youtube.com/watch?v=kpM5gCLF1Zc)
-   🎞 [Roman Sachse - Event Sourcing - Do it yourself series](https://www.youtube.com/playlist?list=PL-nSd-yeckKh7Ts5EKChek7iXcgyUGDHa)
-   📝 [Jay Kreps - Why local state is a fundamental primitive in stream processing](https://www.oreilly.com/ideas/why-local-state-is-a-fundamental-primitive-in-stream-processing)
-   📝 [Jay Kreps - The Log: What every software engineer should know about real-time data's unifying abstraction](https://engineering.linkedin.com/distributed-systems/log-what-every-software-engineer-should-know-about-real-time-datas-unifying)
-   🎞 [Duncan Jones - Event Sourcing and CQRS on Azure serverless functions](https://www.youtube.com/watch?v=jIbfm9TuzIE)
-   📝 [Christian Stettler - Domain Events vs. Event Sourcing](https://www.innoq.com/en/blog/domain-events-versus-event-sourcing/)
-   🎞 [Martin Fowler - The Many Meanings of Event-Driven Architecture](https://www.youtube.com/watch?v=STKCRSUsyP0&t=822s)
-   📝 [Martin Fowler - Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
-   📝 [Dennis Doomen - 16 design guidelines for successful Event Sourcing](https://www.continuousimprover.com/2020/06/guidelines-event-sourcing.html)
-   🎞 [Martin Kleppmann - Event Sourcing and Stream Processing at Scale](https://www.youtube.com/watch?v=avi-TZI9t2I)
-   📝 [Dennis Doomen - The Good, The Bad and the Ugly of Event Sourcing](https://www.continuousimprover.com/2017/11/event-sourcing-good-bad-and-ugly.html)
-   🎞 [Alexey Zimarev - DDD, Event Sourcing and Actors](https://www.youtube.com/watch?v=58_Ehl3oETY)
-   🎞 [Thomas Bøgh Fangel - Event Sourcing: Traceability, Consistency, Correctness](https://www.youtube.com/watch?v=Q-RGrWTN5M4)
-   📝 [Joseph Choe - Event Sourcing, Part 1: User Registration](https://josephchoe.com/event-sourcing-part-1)
-   🎞 [Steven Van Beelen - Intro to Event-Driven Microservices using DDD, CQRS & Event sourcing](https://www.youtube.com/watch?v=F0g5B4F9MMs)
-   📝 [Yves Lorphelin - The Inevitable Event-Centric Book ](https://github.com/ylorph/The-Inevitable-Event-Centric-Book/issues)
-   📝 [Microsoft - Exploring CQRS and Event Sourcing](https://docs.microsoft.com/en-us/previous-versions/msp-n-p/jj554200(v=pandp.10))


<a href='#event-sourcing-on-production' id='event-sourcing-on-production' class='anchor' aria-hidden='true'></a>

### 13.2 Event Sourcing on production
-   🎞 [Alexey Zimarev - Event Sourcing in Production](https://youtu.be/DDefPUCB9ao?t=238)
-   📝 [Leo Gorodinski - Scaling Event-Sourcing at Jet](https://medium.com/@eulerfx/scaling-event-sourcing-at-jet-9c873cac33b8)
-   📝 [EventStoreDB - Customers' case studies](https://www.eventstore.com/case-studies)
-   🎞 [P. Avery, R. Reta - Scaling Event Sourcing for Netflix Downloads](https://www.youtube.com/watch?v=rsSld8NycCU)
-   📝 [Netflix - Scaling Event Sourcing for Netflix Downloads, Episode 1](https://netflixtechblog.com/scaling-event-sourcing-for-netflix-downloads-episode-1-6bc1595c5595)
-   📝 [Netflix - Scaling Event Sourcing for Netflix Downloads, Episode 2](https://netflixtechblog.com/scaling-event-sourcing-for-netflix-downloads-episode-2-ce1b54d46eec)
-   📝 [M. Overeem, M. Spoor, S. Jansen, S. Brinkkemper - An Empirical Characterization of Event Sourced Systems and Their Schema Evolution -- Lessons from Industry](https://arxiv.org/abs/2104.01146)
-   🎞 [Michiel Overeem - Event Sourcing after launch](https://www.youtube.com/watch?v=JzWJI8kW2kc)
-   🎞 [Greg Young - A Decade of DDD, CQRS, Event Sourcing](https://m.youtube.com/watch?v=LDW0QWie21s)
-   📝 [M. Kadijk, J. Taal - The beautiful headache called event sourcing](https://engineering.q42.nl/event-sourcing/)
-   📝 [Thomas Weiss - Planet-scale event sourcing with Azure Cosmos DB](https://medium.com/@thomasweiss_io/planet-scale-event-sourcing-with-azure-cosmos-db-48a557757c8d)
-   🎞 [D. Kuzenski, N. Piani - Beyond APIs: Re-architected System Integrations as Event Sourced](https://www.youtube.com/watch?v=MX4_41yLuG0)
-   🎞 [Greg Young - Why Event Sourced Systems Fail](https://www.youtube.com/watch?v=FKFu78ZEIi8)
-   🎞 [Joris Kuipers - Day 2 problems in CQRS and event sourcing](https://www.youtube.com/watch?v=73KxyTUU4nU)
-   🎞 [Kacper Gunia - War Story: How a Large Corporation Used DDD to Replace a Loyalty System](https://www.youtube.com/watch?v=a1pRsAi9UVs)
-   🎞 [Vladik Khononov - The Dark Side of Events](https://www.youtube.com/watch?v=URYPpY3SgS8)
-   📝 [Pedro Costa - Migrating to Microservices and Event-Sourcing: the Dos and Dont's](https://hackernoon.com/migrating-to-microservices-and-event-sourcing-the-dos-and-donts-195153c7487d)
-   🎞 [Dennis Doomen - An Event Sourcing Retrospective - The Good, The Bad and the Ugly](https://www.youtube.com/watch?v=goknSHnTD4M)
-   🎞 [David Schmitz - Event Sourcing You are doing it wrong](https://www.youtube.com/watch?v=GzrZworHpIk)
-   📝 [Dennis Doomen - A recipe for gradually migrating from CRUD to Event Sourcing](https://www.eventstore.com/blog/a-recipe-for-gradually-migrating-from-crud-to-event-sourcing)
-   🎞 [Nat Pryce - Mistakes made adopting event sourcing (and how we recovered)](https://www.youtube.com/watch?v=osk0ZBdBbx4)

### 13.3 Projections 
-   📝 [Alexey Zimarev - Projections in Event Sourcing](https://zimarev.com/blog/event-sourcing/projections/)
-   📝 [Rinat Abdulin - Event Sourcing - Projections](https://abdullin.com/post/event-sourcing-projections/)
-   🎞 [Derek Comartin - Projections in Event Sourcing: Build ANY model you want!](https://www.youtube.com/watch?v=bTRjO6JK4Ws)

### 13.4 Snapshots
-   📝 [Kacper Gunia - Event Sourcing: Snapshotting](https://domaincentric.net/blog/event-sourcing-snapshotting)
-   🎞 [Derek Comartin - Event Sourcing: Rehydrating Aggregates with Snapshots](https://www.youtube.com/watch?v=eAIkomEid1Y)

### 13.5 Versioning
-   📝 [Greg Young - Versioning in an Event Sourced System](https://leanpub.com/esversioning/read)
-   📝 [Kacper Gunia - Event Sourcing: Snapshotting](https://domaincentric.net/blog/event-sourcing-snapshotting)
-   📝 [M. Overeem, M. Spoor - The dark side of event sourcing: Managing data conversion](https://www.researchgate.net/publication/315637858_The_dark_side_of_event_sourcing_Managing_data_conversion)
-   📝 [Savvas Kleanthous - Event immutability and dealing with change](https://www.eventstore.com/blog/event-immutability-and-dealing-with-change)
-   📝 [Versioning in an Event Sourced System](http://blog.approache.com/2019/02/versioning-in-event-sourced-system-tldr_10.html?m=1)

### 13.6 Storage
-   📝 [Greg Young - Building an Event Storage](https://cqrs.wordpress.com/documents/building-event-storage/)
-   📝 [Adam Warski - Implementing event sourcing using a relational database](https://softwaremill.com/implementing-event-sourcing-using-a-relational-database/)
-   🎞 [Greg Young - How an EventStore actually works](https://www.youtube.com/watch?v=YUjO1wM0PZM)
-   🎞 [Andrii Litvinov - Event driven systems backed by MongoDB](https://www.youtube.com/watch?v=w8Z-kPz1cXw)
-   📝 [Dave Remy - Turning the database inside out with Event Store](https://www.eventstore.com/blog/turning-the-database-inside-out)
-   📝 [AWS Architecture Blog - How The Mill Adventure Implemented Event Sourcing at Scale Using DynamoDB](https://aws.amazon.com/blogs/architecture/how-the-mill-adventure-implemented-event-sourcing-at-scale-using-dynamodb/)
-   🎞 [Sander Molenkamp: Practical CQRS and Event Sourcing on Azure](https://www.youtube.com/watch?v=3XcB-5CrRe8)

### 13.7 Design & Modeling
-   📝 [Mathias Verraes - DDD and Messaging Architectures](http://verraes.net/2019/05/ddd-msg-arch/)
-   📝 [David Boike - Putting your events on a diet](https://particular.net/blog/putting-your-events-on-a-diet)
-   🎞 [Thomas Pierrain - As Time Goes By… (a Bi-temporal Event Sourcing story)](https://youtube.com/watch?v=xzekp1RuZbM)
-   📝 [Vaughn Vernon - Effective Aggregate Design Part I: Modeling a Single Aggregate](https://dddcommunity.org/wp-content/uploads/files/pdf_articles/Vernon_2011_1.pdf)
-   🎞 [Derek Comartin - Aggregate (Root) Design: Separate Behavior & Data for Persistence](https://www.youtube.com/watch?v=GtWVGJp061A)
-   🎞 [Mauro Servienti - All our aggregates are wrong](https://www.youtube.com/watch?v=hev65ozmYPI)
-   📝 [Microsoft - Domain events: design and implementation](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/domain-events-design-implementation)
-   📝 [Event Storming](https://leanpub.com/introducing_eventstorming)
-   📝 [Event Modeling](https://eventmodeling.org/posts/what-is-event-modeling/)
-   📝 [Wojciech Suwała - Building Microservices On .NET Core – Part 5 Marten An Ideal Repository For Your Domain Aggregates](https://altkomsoftware.pl/en/blog/building-microservices-domain-aggregates/)

### 13.8 GDPR
-   📝 [Michiel Rook - Event sourcing and the GDPR: a follow-up](https://www.michielrook.nl/2017/11/event-sourcing-gdpr-follow-up/)

### 13.9 Conflict Detection
-   🎞 [James Geall - Conflict Detection and Resolution in an EventSourced System](https://www.youtube.com/watch?v=-zaa6FUYIQM)
-   🎞 [Lightbend - Data modelling for Replicated Event Sourcing](https://www.youtube.com/watch?v=8PnJxTlOP6o)
-   📰 [Bartosz Sypytkowski - Collaborative Event Sourcing](https://www.slideshare.net/BartoszSypytkowski1/collaborative-replication)

### 13.10 Functional programming
-   📝 [Jérémie Chassaing - Functional Programming and Event Sourcing](https://www.youtube.com/watch?v=kgYGMVDHQHs)

### 13.12 Testing
-   🎞 [N. Rauch & A. Bailly - From Front to Back: Homomorphic Event Sourcing](https://www.youtube.com/watch?v=KyOvBQ87aP4)

### 13.13 CQRS
-   📝 [Greg Young - CQRS](https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf)
-   📝 [Jimmy Bogard - CQRS and REST: the perfect match](https://lostechies.com/jimmybogard/2016/06/01/cqrs-and-rest-the-perfect-match/)
-   📝 [Mark Seemann - CQS versus server-generated IDs](http://blog.ploeh.dk/2014/08/11/cqs-versus-server-generated-ids/)
-   📝 [Julie Lerman - Data Points - CQRS and EF Data Models](https://msdn.microsoft.com/en-us/magazine/mt788619.aspx)
-   📝 [Marco Bürckel - Some thoughts on using CQRS without Event Sourcing](https://medium.com/@mbue/some-thoughts-on-using-cqrs-without-event-sourcing-938b878166a2)
-   📝 [Bertrand Meyer - Eiffel: a language for software engineering (CQRS introduced)](http://laser.inf.ethz.ch/2012/slides/Meyer/eiffel_laser_2013.pdf)
-   🎞 [Udi Dahan - CQRS – but different](https://vimeo.com/131199089)
-   📝 [Greg Young - CQRS, Task Based UIs, Event Sourcing agh!](http://codebetter.com/gregyoung/2010/02/16/cqrs-task-based-uis-event-sourcing-agh/)

### 13.14 Tools
-   🛠️ [Marten - .NET Transactional Document DB and Event Store on PostgreSQL](https://eventuous.dev/)
-   🛠️ [EventStoreDB - The stream database built for Event Sourcing ](https://developers.eventstore.com/)
-   🛠️ [GoldenEye - The CQRS flavoured framework that will speed up your WebAPI and Microservices development ](https://eventuous.dev/)
-   🛠️ [Eventuous - Event Sourcing for .NET](https://eventuous.dev/)
-   🛠️ [SQLStreamStore - Stream Store library targeting RDBMS based implementations for .NET ](https://github.com/SQLStreamStore/SQLStreamStore)
-   🛠️ [Equinox - .NET Event Sourcing library with CosmosDB, EventStoreDB, SqlStreamStore and integration test backends](https://github.com/jet/equinox)

### 13.15 Event processing
-   📝 [Kamil Grzybek - The Outbox Pattern](http://www.kamilgrzybek.com/design/the-outbox-pattern/)
-   🎞 [Dev Mentors - Inbox & Outbox pattern - transactional message processing](https://www.youtube.com/watch?v=ebyR5RPKciw)
-   📝 [Jeremy D. Miller - Jasper's "Outbox" Pattern Support](https://jeremydmiller.com/2018/04/16/jaspers-outbox-pattern-support/)
-   📝 [Gunnar Morling  - Reliable Microservices Data Exchange With the Outbox Pattern](https://debezium.io/blog/2019/02/19/reliable-microservices-data-exchange-with-the-outbox-pattern/)
-   📝 [Microsoft - Asynchronous message-based communication](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/asynchronous-message-based-communication)
-   📝 [NServiceBus - Outbox](https://docs.particular.net/nservicebus/outbox/)
-   📝 [Alvaro Herrera - Implement SKIP LOCKED for row-level locks](https://www.depesz.com/2014/10/10/waiting-for-9-5-implement-skip-locked-for-row-level-locks/)

### 13.16 Distributed processes
-   📝 [Héctor García-Molina, Kenneth Salem - Sagas](https://www.cs.cornell.edu/andru/cs711/2002fa/reading/sagas.pdf)
-   🎞 [Caitie McCaffrey - Applying the Saga Pattern](https://www.youtube.com/watch?v=xDuwrtwYHu8)
-   🎞 [Udi Dahan - If (domain logic) then CQRS or Saga?](https://www.youtube.com/watch?v=fWU8ZK0Dmxs&app=desktop)
-   🎞 [Gregor Hohpe - Starbucks Does Not Use Two-Phase Commit](https://www.enterpriseintegrationpatterns.com/ramblings/18_starbucks.html)
-   📝 [Microsoft - Design Patterns - Saga distributed transactions pattern](https://docs.microsoft.com/en-us/azure/architecture/reference-architectures/saga/saga)
-   📝 [Microsoft - Design Patterns - Choreography](https://docs.microsoft.com/en-us/azure/architecture/patterns/choreography)
-   🎞 [Martin Schimak - Know the Flow! Events, Commands & Long-Running Services](https://www.youtube.com/watch?v=uSF5hyfez60) 
-   📝 [Martin Schimak - Aggregates and Sagas are Processes](https://medium.com/plexiti/aggregates-and-sagas-are-process-owners-e8c8ba973da7)
-   🎞 [Chris Richardson - Using sagas to maintain data consistency in a microservice architecture](https://www.youtube.com/watch?v=YPbGW3Fnmbc)
-   📝 [Thanh Le - What is SAGA Pattern and How important is it?](https://medium.com/swlh/microservices-architecture-what-is-saga-pattern-and-how-important-is-it-55f56cfedd6b)
-   📝 [Jimmy Bogard - Life Beyond Distributed Transactions: An Apostate's Implementation - Relational Resources](https://jimmybogard.com/life-beyond-distributed-transactions-an-apostates-implementation-relational-resources/)
-   📝 [Rinat Abdullin - Evolving Business Processes](https://abdullin.com/post/ddd-evolving-business-processes-a-la-lokad/)
-   📝 [Microsoft - A Saga on Sagas](https://docs.microsoft.com/en-us/previous-versions/msp-n-p/jj591569(v=pandp.10))
-   📝 [NServiceBus - Sagas](https://docs.particular.net/nservicebus/sagas/)
-   📝 [NServiceBus sagas: Integrations](https://docs.particular.net/tutorials/nservicebus-sagas/3-integration)
-   📝 [Denis Rosa (Couchbase) - Saga Pattern | Application Transactions Using Microservices](https://blog.couchbase.com/saga-pattern-implement-business-transactions-using-microservices-part/)

### 13.17 Domain Driven Design
-   📖 [Eric Evans - DDD Reference](https://www.domainlanguage.com/ddd/reference/)
-   📝 [Eric Evans - DDD and Microservices: At Last, Some Boundaries!](https://www.infoq.com/presentations/ddd-microservices-2016)
-   📖 [Domain-Driven Design: The First 15 Years](https://leanpub.com/ddd_first_15_years/)
-   🎞 [Jimmy Bogard - Domain-Driven Design: The Good Parts](https://www.youtube.com/watch?v=U6CeaA-Phqo)
-   💻 [Jakub Pilimon - DDD by Examples](https://github.com/ddd-by-examples/library)
-   📖 [DDD Quickly](https://www.infoq.com/minibooks/domain-driven-design-quickly)
-   📝 [Vaughn Vernon - Reactive DDD: Modeling Uncertainty](https://www.infoq.com/presentations/reactive-ddd-distributed-systems)

### 13.18 Whitepapers
-   📖 [Pat Helland - Immutability Changes Everything](http://cidrdb.org/cidr2015/Papers/CIDR15_Paper16.pdf)
-   📖 [C. Mohan, D. Haderle, B. Lindsay, H. Pirahesh and P. Schwarz - ARIES: A Transaction Recovery Method Supporting Fine-Granularity Locking and Partial Rollbacks Using Write-Ahead Logging](http://db.csail.mit.edu/madden/html/aries.pdf)
-   📖 [P. O'Neil, E. Cheng, D. Gawlick, E. O'Neil - The Log-Structured Merge-Tree (LSM-Tree)](https://www.cs.umb.edu/~poneil/lsmtree.pdf)
-   📖 [S. Copei, A. Zündorf - Commutative Event Sourcing vs. Triple Graph Grammars](https://arxiv.org/abs/2101.08626)

<a href='#this-is-not-event-sourcing' id='this-is-not-event-sourcing' class='anchor' aria-hidden='true'></a>

### 13.19 This is NOT Event Sourcing (but Event Streaming)
-   📝 [Confluent - Event sourcing, CQRS, stream processing and Apache Kafka: What's the connection?](https://www.confluent.io/blog/event-sourcing-cqrs-stream-processing-apache-kafka-whats-connection/)
-   🎞 [InfoQ - Building Microservices with Event Sourcing and CQRS](https://www.infoq.com/presentations/microservices-event-sourcing-cqrs/)
-   📝 [Chris Kiehl - Don't Let the Internet Dupe You, Event Sourcing is Hard](https://chriskiehl.com/article/event-sourcing-is-hard)
-   📝 [AWS - Event sourcing pattern](https://docs.aws.amazon.com/prescriptive-guidance/latest/modernization-data-persistence/service-per-team.html)
-   📝 [Andela - Building Scalable Applications Using Event Sourcing and CQRS](https://andela.com/insights/building-scalable-applications-using-event-sourcing-and-cqrs/)
-   📝 [WiX Engineering - The Reactive Monolith - How to Move from CRUD to Event Sourcing](https://www.wix.engineering/post/the-reactive-monolith-how-to-move-from-crud-to-event-sourcing)
-   📝 [Nexocode - CQRS and Event Sourcing as an antidote for problems with retrieving application states](https://nexocode.com/blog/posts/cqrs-and-event-sourcing/)
-   📝 [coMakeIT - Event sourcing and CQRS in Action](https://www.comakeit.com/blog/event-sourcing-and-cqrs-in-action/)
-   📝 [Debezium - Distributed Data for Microservices — Event Sourcing vs. Change Data Capture](https://debezium.io/blog/2020/02/10/event-sourcing-vs-cdc/)
-   📝 [Codurance - CQRS and Event Sourcing for dummies](https://www.codurance.com/publications/2015/07/18/cqrs-and-event-sourcing-for-dummies)
-   📝 [Slalom Build - Event Sourcing with AWS Lambda](https://www.slalombuild.com/blueprint-articles/lambda)
-   📝 [AWS Prescriptive Guidance - Decompose monoliths into microservices by using CQRS and event sourcing](https://docs.aws.amazon.com/prescriptive-guidance/latest/patterns/decompose-monoliths-into-microservices-by-using-cqrs-and-event-sourcing.html)
-   📝 [Zartis - Event Sourcing with CQRS](https://www.zartis.com/event-sourcing-with-cqrs/)
-   📝 [Nordstrom - Event-sourcing at Nordstrom: Part 1](https://medium.com/tech-at-nordstrom/adventures-in-event-sourced-architecture-part-1-cc21d06187c7)
-   📝 [Nordstrom - Event-sourcing at Nordstrom: Part 2](https://medium.com/tech-at-nordstrom/event-sourcing-at-nordstrom-part-2-f64c416d1885)
-   🎞 [Techtter - CQRS - Event Sourcing || Deep Dive on Building Event Driven Systems](https://www.youtube.com/watch?v=3TwLEoLtpw0)
-   🎞 [Tech Mind Factory - Event Sourcing with Azure SQL and Entity Framework Core](https://www.youtube.com/watch?v=-BhDW3GeSqg)
-   🎞 [Tech Primers - Event Sourcing & CQRS | Stock Exchange Microservices Architecture | System Design Primer](https://www.youtube.com/watch?v=E-7TBZxmkXE)
-   🎞 [International JavaScript Conference - DDD, event sourcing and CQRS – theory and practice](https://www.youtube.com/watch?v=rolfJR9ERxo)
-   🎞 [Event Sourcing in NodeJS / Typescript - ESaucy](https://www.youtube.com/watch?v=3TMRIzxWF_8)
-   🎞 [Kansas City Spring User Group - Event Sourcing from Scratch with Apache Kafka and Spring](https://www.youtube.com/watch?v=pRUxU5OSB0c)
-   🎞 [jeeconf - Building event sourced systems with Kafka Streams](https://www.youtube.com/watch?v=b17l7LvrTco)
-   🎞 [Jfokus - Event sourcing in practise - lessons learned](https://www.youtube.com/watch?v=_d4mAi3qkDA)
-   🎞 [MecaHumArduino - Event Sourcing on AWS - Serverless Patterns YOU HAVE To Know About](https://www.youtube.com/watch?v=NvuZoDfuoBc)
-   🎞 [Oracle Developers - Event Sourcing, Distributed Systems, and CQRS with Java EE](https://www.youtube.com/watch?v=yql-VL1rJWY)
-   🎞 [Creating a Blueprint for Microservices and Event Sourcing on AWS](https://itnext.io/creating-a-blueprint-for-microservices-and-event-sourcing-on-aws-291d4d5a5817)
-   🎞 [Azure Cosmos DB Conf - Implementing an Event Sourcing strategy on Azure](https://channel9.msdn.com/Events/Azure-Cosmos-DB/Azure-Cosmos-DB-Conf/Implementing-an-Event-Sourcing-strategy-on-Azure)
-   📝 [CosmosDB DevBlog - Create a Java Azure Cosmos DB Function Trigger using Visual Studio Code in 2 minutes!](https://devblogs.microsoft.com/cosmosdb/create-a-java-azure-cosmos-db-function-trigger-using-visual-studio-code-in-2-minutes/)
-   📝 [Towards Data Science - The Design of an Event Store](https://towardsdatascience.com/the-design-of-an-event-store-8c751c47db6f)
-   📝 [Aspnetrun - CQRS and Event Sourcing in Event Driven Architecture of Ordering Microservices](https://medium.com/aspnetrun/cqrs-and-event-sourcing-in-event-driven-architecture-of-ordering-microservices-fb67dc44da7a)
-   📝 [Why Microservices Should use Event Sourcing](https://blog.bitsrc.io/why-microservices-should-use-event-sourcing-9755a54ebfb4)

### 13.20 Event Sourcing Concerns
-   📝 [Kacper Gunia - EventStoreDB vs Kafka](https://domaincentric.net/blog/eventstoredb-vs-kafka)
-   📝 [Vijay Nair - Axon and Kafka - How does Axon compare to Apache Kafka?](https://axoniq.io/blog-overview/axon-and-kafka-how-does-axon-compare-to-apache-kafka)
-   📝 [Jesper Hammarbäck - Apache Kafka is not for Event Sourcing](https://serialized.io/blog/apache-kafka-is-not-for-event-sourcing/)
-   🎞 [Udi Dahan - Event Sourcing @ DDDEU 2020 Keynote](https://channel9.msdn.com/Events/Azure-Cosmos-DB/Azure-Cosmos-DB-Conf/Implementing-an-Event-Sourcing-strategy-on-Azure)
-   📝 [Vikas Hazrati - Event Sourcing – Does it make sense for your business?](https://blog.knoldus.com/event-sourcing-does-it-make-sense-for-your-business/)
-   📝 [Mikhail Shilkov - Event Sourcing and IO Complexity](https://mikhail.io/2016/11/event-sourcing-and-io-complexity/)
-   📝 [Dennis Doomen - The Ugly of Event Sourcing - Real-world Production Issues](https://www.linkedin.com/pulse/ugly-event-sourcing-real-world-production-issues-dennis-doomen/)
-   📝 [Hugo Rocha - What they don’t tell you about event sourcing](https://medium.com/@hugo.oliveira.rocha/what-they-dont-tell-you-about-event-sourcing-6afc23c69e9a)
-   📝 [Oskar uit de Bos - Stop overselling Event Sourcing as the silver bullet to microservice architectures](https://medium.com/swlh/stop-overselling-event-sourcing-as-the-silver-bullet-to-microservice-architectures-f43ca25ff9e7)

### 13.21 Architecture Weekly
If you're interested in Architecture resources, check my other repository: https://github.com/oskardudycz/ArchitectureWeekly/.

It contains a weekly updated list of materials I found valuable and educational.

---

**EventSourcing.NetCore** is Copyright &copy; 2017-2022 [Oskar Dudycz](http://event-driven.io) and other contributors under the [MIT license](LICENSE).
