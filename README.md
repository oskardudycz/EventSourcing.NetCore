[![Twitter Follow](https://img.shields.io/twitter/follow/oskar_at_net?style=social)](https://twitter.com/oskar_at_net) [![Join the chat at https://gitter.im/EventSourcing-NetCore/community](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/EventSourcing-NetCore/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) ![Github Actions](https://github.com/oskardudycz/EventSourcing.NetCore/actions/workflows/build.dotnet.yml/badge.svg?branch=main) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_net)

# EventSourcing.NetCore

Tutorial, practical samples and other resources about Event Sourcing in .NET Core.

- [EventSourcing.NetCore](#eventsourcingnetcore)
  - [1. Event Sourcing](#1-event-sourcing)
    - [1.1 What is Event Sourcing?](#11-what-is-event-sourcing)
    - [1.2 What is Event?](#12-what-is-event)
    - [1.3 What is Stream?](#13-what-is-stream)
    - [1.4 Event representation](#14-event-representation)
    - [1.5 Event Storage](#15-event-storage)
    - [1.6 Retrieving the current state from events](#16-retrieving-the-current-state-from-events)
  - [2. Support](#2-support)
  - [3. Prerequisites](#3-prerequisites)
  - [4. Tools used](#4-tools-used)
  - [5. Samples](#5-samples)
  - [6. Self-paced training Kit](#6-self-paced-training-kit)
  - [7. Articles](#7-articles)
  - [8. Event Store - Marten](#8-event-store---marten)
  - [9. Message Bus (for processing Commands, Queries, Events) - MediatR](#9-message-bus-for-processing-commands-queries-events---mediatr)
  - [10. CQRS (Command Query Responsibility Separation)](#10-cqrs-command-query-responsibility-separation)
  - [11. NuGet packages to help you get started.](#11-nuget-packages-to-help-you-get-started)
  - [12. Other resources](#12-other-resources)
    - [12.1 Introduction](#121-introduction)
    - [12.2 Event Sourcing on production](#122-event-sourcing-on-production)
    - [12.3 Projections](#123-projections)
    - [12.4 Snapshots](#124-snapshots)
    - [12.5 Versioning](#125-versioning)
    - [12.6 Storage](#126-storage)
    - [12.7 Design & Modeling](#127-design--modeling)
    - [12.8 GDPR](#128-gdpr)
    - [12.9 Conflict Detection](#129-conflict-detection)
    - [12.10 Functional programming](#1210-functional-programming)
    - [12.12 Testing](#1212-testing)
    - [12.13 CQRS](#1213-cqrs)
    - [12.14 Tools](#1214-tools)
    - [12.15 Event Sourcing vs Messaging](#1215-event-sourcing-vs-messaging)
    - [12.15 Event processing](#1215-event-processing)
    - [12.16 Distributed processes](#1216-distributed-processes)
    - [12.17 Domain Driven Design](#1217-domain-driven-design)
    - [12.18 Architecture Weekly](#1218-architecture-weekly)


## 1. Event Sourcing

### 1.1 What is Event Sourcing?

Event Sourcing is a design pattern in which results of business operations are stored as a series of events. 

It is an alternative way to persist data. In contrast with state-oriented persistence that only keeps the latest version of the entity state, Event Sourcing stores each state change as a separate event.

Thanks for that, no business data is lost. Each operation results in the event stored in the database. That enables extended auditing and diagnostics capabilities (both technically and business-wise). What's more, as events contains the business context, it allows wide business analysis and reporting.

In this repository I'm showing different aspects, patterns around Event Sourcing. From the basic to advanced practices.

Read more in my articles:
-   📝 [How using events helps in a teams' autonomy](https://event-driven.io/en/how_using_events_help_in_teams_autonomy/?utm_source=event_sourcing_net)
-   📝 [When not to use Event Sourcing?](https://event-driven.io/en/when_not_to_use_event_sourcing/?utm_source=event_sourcing_net)

### 1.2 What is Event?

Events, represent facts in the past. They carry information about something accomplished. It should be named in the past tense, e.g. _"user added"_, _"order confirmed"_. Events are not directed to a specific recipient - they're broadcasted information. It's like telling a story at a party. We hope that someone listens to us, but we may quickly realise that no one is paying attention.

Events:
- are immutable: _"What has been seen, cannot be unseen"_.
- can be ignored but cannot be retracted (as you cannot change the past).
- can be interpreted differently. The basketball match result is a fact. Winning team fans will interpret it positively. Losing team fans - not so much.

Read more in my articles:
-   📝 [What's the difference between a command and an event?](https://event-driven.io/en/whats_the_difference_between_event_and_command/?utm_source=event_sourcing_net)
-   📝 [Events should be as small as possible, right?](https://event-driven.io/en/events_should_be_as_small_as_possible/?utm_source=event_sourcing_net)

### 1.3 What is Stream?

Events are logically grouped into streams. In Event Sourcing, streams are the representation of the entities. All the entity state mutations ends up as the persisted events. Entity state is retrieved by reading all the stream events and applying them one by one in the order of appearance.

A stream should have a unique identifier representing the specific object. Each event has its own unique position within a stream. This position is usually represented by a numeric, incremental value. This number can be used to define the order of the events while retrieving the state. It can be also used to detect concurrency issues. 

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

Event Sourcing is not related to any type of storage implementation. As long as it fulfils the assumptions, it can be implemented having any backing database (relational, document, etc.). The state has to be represented by the append-only log of events. The events are stored in chronological order, and new events are appended to the previous event. Event Stores are the databases' category explicitly designed for such purpose. 

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

All of those events shares the stream id (`"streamId": "INV/2021/11/01"`), and have incremented stream position.

We can get to conclusion that in Event Sourcing entity is represented by stream, so sequence of event correlated by the stream id ordered by stream position.

To get the current state of entity we need to perform the stream aggregation process. We're translating the set of events into a single entity. This can be done with the following the steps:
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

## 2. Support

Feel free to [create an issue](https://github.com/oskardudycz/EventSourcing.NetCore/issues/new) if you have any questions or request for more explanation or samples. I also take **Pull Requests**!

💖 If this repository helped you - I'd be more than happy if you **join** the group of **my official supporters** at:

👉 [Github Sponsors](https://github.com/sponsors/oskardudycz) 

## 3. Prerequisites

For running the Event Store examples you need to have:

1. .NET 5 installed - https://dotnet.microsoft.com/download/dotnet/5.0
2. [Docker](https://store.docker.com/search?type=edition&offering=community) installed. Then going to the `docker` folder and running:

```
docker-compose up
```

**More information about using .NET Core, WebApi and Docker you can find in my other tutorials:** [WebApi with .NET](https://github.com/oskardudycz/WebApiWith.NETCore)

You can also watch my presentation _"Practical Event Sourcing with Marten"_:

<a href="https://www.youtube.com/watch?v=L_ized5xwww&list=PLw-VZz_H4iio9b_NrH25gPKjr2MAS2YgC&index=7" target="_blank"><img src="https://img.youtube.com/vi/L_ized5xwww/0.jpg" alt="Practical Event Sourcing with Marten (EN)" width="320" height="240" border="10" /></a>

and discussion with [Yves Lorphelin](https://github.com/ylorph/) about CQRS:

<a href="https://www.youtube.com/watch?v=D-3N2vQ7ADE" target="_blank"><img src="https://img.youtube.com/vi/D-3N2vQ7ADE/0.jpg" alt="Event Store Conversations: Yves Lorphelin talks to Oskar Dudycz about CQRS (EN)" width="320" height="240" border="10" /></a>

## 4. Tools used

1. [Marten](https://martendb.io/) - Event Store and Read Models
2. [EventStoreDB](https://eventstore.com) - Event Store 
3. [MediatR](https://github.com/jbogard/MediatR) - Internal In-Memory Message Bus (for processing Commands, Queries, Events)
4. [Kafka](https://github.com/jbogard/MediatR) - External Durable Message Bus to integrate services
5. [ElasticSearch](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/elasticsearch-net-getting-started.html) - Read Models

## 5. Samples

See also fully working, real-world samples of Event Sourcing and CQRS applications in [Samples folder](https://github.com/oskardudycz/EventSourcing.NetCore/tree/main/Sample).

- **[ECommerce with Marten](./Sample/ECommerce)**
  - typical Event Sourcing and CQRS flow,
  - DDD using Aggregates,
  - microservices example,
  - stores events to Marten,
  - distributed processes coordinated by Saga ([Order Saga](./Sample/ECommerce/Orders/Orders/Orders/OrderSaga.cs)),
  - Kafka as a messaging platform to integrate microservices,
  - example of the case when some services are event-sourced ([Carts](./Sample/ECommerce/Carts), [Orders](./Sample/ECommerce/Orders), [Payments](./Sample/ECommerce/Payments)) and some are not ([Shipments](./Sample/ECommerce/Shipments) using EntityFramework as ORM)

- **[ECommerce with EventStoreDB](./Sample/EventStoreDB/ECommerce)** 
  - typical Event Sourcing and CQRS flow,
  - DDD using Aggregates,
  - stores events to  EventStoreDB,
  - Builds read models using [Subscription to `$all`](https://developers.eventstore.com/clients/grpc/subscribing-to-streams/#subscribing-to-all).
  - Read models are stored as Marten documents.

- **[Warehouse](./Sample/Warehouse)**
  - simplest CQRS flow using .NET 5 Endpoints,
  - example of how and where to use C# Records, Nullable Reference Types, etc,
  - No Event Sourcing! Using Entity Framework to show that CQRS is not bounded to Event Sourcing or any type of storage,
  - No Aggregates! CQRS do not need DDD. Business logic can be handled in handlers.

- **[Meetings Management with Marten](./Sample/MeetingsManagement/)** 
  - typical Event Sourcing and CQRS flow,
  - DDD using Aggregates,
  - microservices example,
  - stores events to Marten,
  - Kafka as a messaging platform to integrate microservices,
  - read models handled in separate microservice and stored to other database (ElasticSearch)

- **[Cinema Tickets Reservations with Marten](./Sample/Tickets/)**
  - typical Event Sourcing and CQRS flow,
  - DDD using Aggregates,
  - stores events to Marten.

- **[SmartHome IoT with Marten](./Sample/AsyncProjections/)**
  - typical Event Sourcing and CQRS flow,
  - DDD using Aggregates,
  - stores events to Marten,
  - asynchronous projections rebuild using AsynDaemon feature.

## 6. Self-paced training Kit

I prepared the self-paced training Kit for the Event Sourcing. See more in the [Workshop description](./Workshops/BuildYourOwnEventStore/Readme.md).

**Event Sourcing basics** - it teaches the event store basics by showing how to build your Event Store on Relational Database. It starts with the tables setup, goes through appending events, aggregations, projections, snapshots, and finishes with the `Marten` basics. See more in [here](./Workshops/BuildYourOwnEventStore/).

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

## 7. Articles
Read also more on the **Event Sourcing** and **CQRS** topics in my [blog](https://event-driven.io/?utm_source=event_sourcing_net) posts:
-   📝 [What's the difference between a command and an event?](https://event-driven.io/en/whats_the_difference_between_event_and_command/?utm_source=event_sourcing_net)
-   📝 [Events should be as small as possible, right?](https://event-driven.io/en/events_should_be_as_small_as_possible/?utm_source=event_sourcing_net)
-   📝 [How to get the current entity state from events?](https://event-driven.io/en/how_to_get_the_current_entity_state_in_event_sourcing/?utm_source=event_sourcing_net)
-   📝 [Why a bank account is not the best example of Event Sourcing?](https://event-driven.io/en/bank_account_event_sourcing/?utm_source=event_sourcing_net)
-   📝 [How using events helps in a teams' autonomy](https://event-driven.io/en/how_using_events_help_in_teams_autonomy/?utm_source=event_sourcing_net)
-   📝 [When not to use Event Sourcing?](https://event-driven.io/en/when_not_to_use_event_sourcing/?utm_source=event_sourcing_net)
-   📝 [How to (not) do the events versioning?](https://event-driven.io/en/how_to_do_event_versioning/?utm_source=event_sourcing_net)
-   📝 [CQRS facts and myths explained](https://event-driven.io/en/cqrs_facts_and_myths_explained/?utm_source=event_sourcing_net)
-   📝 [Can command return a value?](https://event-driven.io/en/can_command_return_a_value/?utm_source=event_sourcing_net)
-   📝 [How to create projections of events for nested object structures?](https://event-driven.io/en/how_to_create_projections_of_events_for_nested_object_structures/?utm_source=event_sourcing_net)
-   📝 [How to scale projections in the event-driven systems?](https://event-driven.io/en/how_to_scale_projections_in_the_event_driven_systems/?utm_source=event_sourcing_net)
-   📝 [What texting your Ex has to do with Event-Driven Design?](https://event-driven.io/en/what_texting_ex_has_to_do_with_event_driven_design/?utm_source=event_sourcing_net)
-   📝 [What if I told you that Relational Databases are in fact Event Stores?](https://event-driven.io/en/relational_databases_are_event_stores/?utm_source=event_sourcing_net)
-   📝 [Optimistic concurrency for pessimistic times](https://event-driven.io/en/optimistic_concurrency_for_pessimistic_times/?utm_source=event_sourcing_net)
-   📝 [Outbox, Inbox patterns and delivery guarantees explained](https://event-driven.io/en/outbox_inbox_patterns_and_delivery_guarantees_explained/?utm_source=event_sourcing_net)
-   📝 [Saga and Process Manager - distributed processes in practice](https://event-driven.io/en/saga_process_manager_distributed_transactions/?utm_source=event_sourcing_net)

Slides:
-   👨🏼‍🏫 **Practical Event Sourcing with Marten** - [EN](./Slides/EventSourcing_with_Marten_EN.pptx), [PL](./Slides/EventSourcing_with_Marten_PL.pptx)
-   👨🏼‍🏫 **Ligths and Shades of Event-Driven Design** -  [EN](./Slides/Lights_And_Shades_Of_Event-Driven_Design.pptx), [PL](./Slides/SegFault-Blaski_i_Cienie.pptx)
-   👨🏼‍🏫 **Adventures in Event Sourcing and CQRS** - [PL](./Slides/Slides.pptx)

## 8. Event Store - Marten

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

## 9. Message Bus (for processing Commands, Queries, Events) - MediatR

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

## 10. CQRS (Command Query Responsibility Separation)

-   **[Command handling](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/CQRS.Tests/Commands/Commands.cs)**
-   **[Query handling](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/CQRS.Tests/Queries/Queries.cs)**

## 11. NuGet packages to help you get started.

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

## 12. Other resources

### 12.1 Introduction
-   📝 [Event Store - A Beginner’s Guide to Event Sourcing](https://www.eventstore.com/event-sourcing)
-   🎞 [Greg Young - CQRS & Event Sourcing](https://youtube.com/watch?v=JHGkaShoyNs)
-   📝 [Jay Kreps - Why local state is a fundamental primitive in stream processing](https://www.oreilly.com/ideas/why-local-state-is-a-fundamental-primitive-in-stream-processing)
-   📝 [Microsoft - Exploring CQRS and Event Sourcing](https://docs.microsoft.com/en-us/previous-versions/msp-n-p/jj554200(v=pandp.10))
-   📰 [Lorenzo Nicora - A visual introduction to event sourcing and cqrs](https://www.slideshare.net/LorenzoNicora/a-visual-introduction-to-event-sourcing-and-cqrs)
-   🎞 [Mathew McLoughlin - An Introduction to CQRS and Event Sourcing Patterns](https://www.youtube.com/watch?v=9a1PqwFrMP0)
-   🎞 [Emily Stamey - Hey Boss, Event Sourcing Could Fix That!](https://www.youtube.com/watch?v=mw7D6OJpsIA)
-   🎞 [Derek Comartin - Event Sourcing Example & Explained in plain English](https://www.youtube.com/watch?v=AUj4M-st3ic)
-   🎞 [Duncan Jones - Introduction to event sourcing and CQRS](https://www.youtube.com/watch?v=kpM5gCLF1Zc)
-   🎞 [Roman Sachse - Event Sourcing - Do it yourself series](https://www.youtube.com/playlist?list=PL-nSd-yeckKh7Ts5EKChek7iXcgyUGDHa)
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

### 12.2 Event Sourcing on production
-   📝 [Leo Gorodinski - Scaling Event-Sourcing at Jet](https://medium.com/@eulerfx/scaling-event-sourcing-at-jet-9c873cac33b8)
-   📝 [EventStoreDB - Customers' case studies](https://www.eventstore.com/case-studies)
-   🎞 [P. Avery, R. Reta - Scaling Event Sourcing for Netflix Downloads](https://www.youtube.com/watch?v=rsSld8NycCU)
-   📝 [M. Overeem, M. Spoor, S. Jansen, S. Brinkkemper - An Empirical Characterization of Event Sourced Systems and Their Schema Evolution -- Lessons from Industry](https://arxiv.org/abs/2104.01146)
-   🎞 [Michiel Overeem - Event Sourcing after launch](https://www.youtube.com/watch?v=JzWJI8kW2kc)
-   🎞 [Greg Young - A Decade of DDD, CQRS, Event Sourcing](https://m.youtube.com/watch?v=LDW0QWie21s)
-   📝 [M. Kadijk, J. Taal - The beautiful headache called event sourcing](https://engineering.q42.nl/event-sourcing/)
-   📝 [Thomas Weiss - Planet-scale event sourcing with Azure Cosmos DB](https://medium.com/@thomasweiss_io/planet-scale-event-sourcing-with-azure-cosmos-db-48a557757c8d)
-   🎞 [D. Kuzenski, N. Piani - Beyond APIs: Re-architected System Integrations as Event Sourced](https://www.youtube.com/watch?v=MX4_41yLuG0)
-   🎞 [Greg Young - Why Event Sourced Systems Fail](https://www.youtube.com/watch?v=FKFu78ZEIi8)
-   🎞 [Kacper Gunia - War Story: How a Large Corporation Used DDD to Replace a Loyalty System](https://www.youtube.com/watch?v=a1pRsAi9UVs)
-   🎞 [Vladik Khononov - The Dark Side of Events](https://www.youtube.com/watch?v=URYPpY3SgS8)
-   📝 [Pedro Costa - Migrating to Microservices and Event-Sourcing: the Dos and Dont’s](https://hackernoon.com/migrating-to-microservices-and-event-sourcing-the-dos-and-donts-195153c7487d)
-   🎞 [Dennis Doomen - An Event Sourcing Retrospective - The Good, The Bad and the Ugly](https://www.youtube.com/watch?v=goknSHnTD4M)
-   🎞 [David Schmitz - Event Sourcing You are doing it wrong](https://www.youtube.com/watch?v=GzrZworHpIk)
-   📝 [Dennis Doomen - A recipe for gradually migrating from CRUD to Event Sourcing](https://www.eventstore.com/blog/a-recipe-for-gradually-migrating-from-crud-to-event-sourcing)

### 12.3 Projections 
-   📝 [Alexey Zimarev - Projections in Event Sourcing](https://zimarev.com/blog/event-sourcing/projections/)
-   📝 [Rinat Abdulin - Event Sourcing - Projections](https://abdullin.com/post/event-sourcing-projections/)
-   🎞 [Derek Comartin - Projections in Event Sourcing: Build ANY model you want!](https://www.youtube.com/watch?v=bTRjO6JK4Ws)

### 12.4 Snapshots
-   📝 [Kacper Gunia - Event Sourcing: Snapshotting](https://domaincentric.net/blog/event-sourcing-snapshotting)
-   🎞 [Derek Comartin - Event Sourcing: Rehydrating Aggregates with Snapshots](https://www.youtube.com/watch?v=eAIkomEid1Y)

### 12.5 Versioning
-   📝 [Greg Young - Versioning in an Event Sourced System](https://leanpub.com/esversioning/read)
-   📝 [Kacper Gunia - Event Sourcing: Snapshotting](https://domaincentric.net/blog/event-sourcing-snapshotting)
-   📝 [M. Overeem, M. Spoor - The dark side of event sourcing: Managing data conversion](https://www.researchgate.net/publication/315637858_The_dark_side_of_event_sourcing_Managing_data_conversion)
-   📝 [Savvas Kleanthous - Event immutability and dealing with change](https://www.eventstore.com/blog/event-immutability-and-dealing-with-change)
-   📝 [Versioning in an Event Sourced System](http://blog.approache.com/2019/02/versioning-in-event-sourced-system-tldr_10.html?m=1)

### 12.6 Storage
-   📝 [Greg Young - Building an Event Storage](https://cqrs.wordpress.com/documents/building-event-storage/)
-   🎞 [Andrii Litvinov - Event driven systems backed by MongoDB](https://www.youtube.com/watch?v=w8Z-kPz1cXw)
-   📝 [Dave Remy - Turning the database inside out with Event Store](https://www.eventstore.com/blog/turning-the-database-inside-out)

### 12.7 Design & Modeling
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

### 12.8 GDPR
-   📝 [Michiel Rook - Event sourcing and the GDPR: a follow-up](https://www.michielrook.nl/2017/11/event-sourcing-gdpr-follow-up/)

### 12.9 Conflict Detection
-   🎞 [James Geall - Conflict Detection and Resolution in an EventSourced System](https://www.youtube.com/watch?v=-zaa6FUYIQM)
-   🎞 [Lightbend - Data modelling for Replicated Event Sourcing](https://www.youtube.com/watch?v=8PnJxTlOP6o)
-   📰 [Bartosz Sypytkowski - Collaborative Event Sourcing](https://www.slideshare.net/BartoszSypytkowski1/collaborative-replication)

### 12.10 Functional programming
-   📝 [Jérémie Chassaing - Functional Programming and Event Sourcing](https://www.youtube.com/watch?v=kgYGMVDHQHs)

### 12.12 Testing
-   🎞 [N. Rauch & A. Bailly - From Front to Back: Homomorphic Event Sourcing](https://www.youtube.com/watch?v=KyOvBQ87aP4)

### 12.13 CQRS
-   📝 [Greg Young - CQRS](https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf)
-   📝 [Jimmy Bogard - CQRS and REST: the perfect match](https://lostechies.com/jimmybogard/2016/06/01/cqrs-and-rest-the-perfect-match/)
-   📝 [Mark Seemann - CQS versus server-generated IDs](http://blog.ploeh.dk/2014/08/11/cqs-versus-server-generated-ids/)
-   📝 [Julie Lerman - Data Points - CQRS and EF Data Models](https://msdn.microsoft.com/en-us/magazine/mt788619.aspx)
-   📝 [Marco Bürckel - Some thoughts on using CQRS without Event Sourcing](https://medium.com/@mbue/some-thoughts-on-using-cqrs-without-event-sourcing-938b878166a2)
-   📝 [Bertrand Meyer - Eiffel: a language for software engineering (CQRS introduced)](http://laser.inf.ethz.ch/2012/slides/Meyer/eiffel_laser_2012.pdf)

### 12.14 Tools
-   🛠️ [Marten - .NET Transactional Document DB and Event Store on PostgreSQL](https://eventuous.dev/)
-   🛠️ [EventStoreDB - The stream database built for Event Sourcing ](https://developers.eventstore.com/)
-   🛠️ [GoldenEye - The CQRS flavoured framework that will speed up your WebAPI and Microservices development ](https://eventuous.dev/)
-   🛠️ [Eventuous - Event Sourcing for .NET](https://eventuous.dev/)
-   🛠️ [SQLStreamStore - Stream Store library targeting RDBMS based implementations for .NET ](https://github.com/SQLStreamStore/SQLStreamStore)
-   🛠️ [Equinox - .NET Event Sourcing library with CosmosDB, EventStoreDB, SqlStreamStore and integration test backends](https://github.com/jet/equinox)

### 12.15 Event Sourcing vs Messaging
-   📝 [Kacper Gunia - EventStoreDB vs Kafka](https://domaincentric.net/blog/eventstoredb-vs-kafka)
-   📝 [Vijay Nair - Axon and Kafka - How does Axon compare to Apache Kafka?](https://axoniq.io/blog-overview/axon-and-kafka-how-does-axon-compare-to-apache-kafka)

### 12.15 Event processing
-   📝 [Kamil Grzybek - The Outbox Pattern](http://www.kamilgrzybek.com/design/the-outbox-pattern/)
-   🎞 [Dev Mentors - Inbox & Outbox pattern - transactional message processing](https://www.youtube.com/watch?v=ebyR5RPKciw)
-   📝 [Jeremy D. Miller - Jasper's "Outbox" Pattern Support](https://jeremydmiller.com/2018/04/16/jaspers-outbox-pattern-support/)
-   📝 [Gunnar Morling  - Reliable Microservices Data Exchange With the Outbox Pattern](https://debezium.io/blog/2019/02/19/reliable-microservices-data-exchange-with-the-outbox-pattern/)
-   📝 [Microsoft - Asynchronous message-based communication](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/asynchronous-message-based-communication)
-   📝 [NServiceBus - Outbox](https://docs.particular.net/nservicebus/outbox/)
-   📝 [Alvaro Herrera - Implement SKIP LOCKED for row-level locks](https://www.depesz.com/2014/10/10/waiting-for-9-5-implement-skip-locked-for-row-level-locks/)

### 12.16 Distributed processes
-   📝 [Hector Garcaa-Molrna, Kenneth Salem - Sagas](https://www.cs.cornell.edu/andru/cs711/2002fa/reading/sagas.pdf)
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

### 12.17 Domain Driven Design
-   📖 [Eric Evans - DDD Reference](https://www.domainlanguage.com/ddd/reference/)
-   📝 [Eric Evans - DDD and Microservices: At Last, Some Boundaries!](https://www.infoq.com/presentations/ddd-microservices-2016)
-   📖 [Domain-Driven Design: The First 15 Years](https://leanpub.com/ddd_first_15_years/)
-   🎞 [Jimmy Bogard - Domain-Driven Design: The Good Parts](https://www.youtube.com/watch?v=U6CeaA-Phqo)
-   💻 [Jakub Pilimon - DDD by Examples](https://github.com/ddd-by-examples/library)
-   📖 [DDD Quickly](https://www.infoq.com/minibooks/domain-driven-design-quickly)
-   📝 [Vaughn Vernon - Reactive DDD: Modeling Uncertainty](https://www.infoq.com/presentations/reactive-ddd-distributed-systems)

### 12.18 Architecture Weekly
If you're interested in Architecture resources, check my other repository: https://github.com/oskardudycz/ArchitectureWeekly/.

It contains a weekly updated list of materials I found valuable and educational.

---

**EventSourcing.NetCore** is Copyright &copy; 2017-2021 [Oskar Dudycz](http://oskar-dudycz.pl) and other contributors under the [MIT license](LICENSE).
