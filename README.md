[![Twitter Follow](https://img.shields.io/twitter/follow/oskar_at_net?style=social)](https://twitter.com/oskar_at_net) [![Join the chat at https://gitter.im/EventSourcing-NetCore/community](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/EventSourcing-NetCore/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) [![Build status](https://ci.appveyor.com/api/projects/status/8gb320jrp40el9ye/branch/main?svg=true)](https://ci.appveyor.com/project/oskardudycz/eventsourcing-netcore/branch/main) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/)

# EventSourcing.NetCore

Tutorial, practical samples and other resources about Event Sourcing in .NET Core.

- [EventSourcing.NetCore](#eventsourcingnetcore)
  - [1. Support](#1-support)
  - [2. Prerequisites](#2-prerequisites)
  - [3. Libraries used](#3-libraries-used)
  - [4. Articles](#4-articles)
  - [5. Event Store - Marten](#5-event-store---marten)
  - [6. Message Bus (for processing Commands, Queries, Events) - MediatR](#6-message-bus-for-processing-commands-queries-events---mediatr)
  - [7. CQRS (Command Query Responsibility Separation)](#7-cqrs-command-query-responsibility-separation)
  - [8. Fully working sample application](#8-fully-working-sample-application)
  - [9. Self-paced training Kit](#9-self-paced-training-kit)
  - [10. NuGet packages to help you get started.](#10-nuget-packages-to-help-you-get-started)
  - [11. Other resources](#11-other-resources)
    - [11.1 Introduction](#111-introduction)
    - [11.2 Event Sourcing on production](#112-event-sourcing-on-production)
    - [11.3 Projections](#113-projections)
    - [11.4 Snapshots](#114-snapshots)
    - [11.5 Versioning](#115-versioning)
    - [11.6 Storage](#116-storage)
    - [11.7 Design & Modeling](#117-design--modeling)
    - [11.8 GDPR](#118-gdpr)
    - [11.9 Conflict Detection](#119-conflict-detection)
    - [11.10 Functional programming](#1110-functional-programming)
    - [11.12 Testing](#1112-testing)
    - [11.13 CQRS](#1113-cqrs)
    - [11.14 Tools](#1114-tools)
    - [11.15 Event Sourcing vs Messaging](#1115-event-sourcing-vs-messaging)
    - [11.15 Event processing](#1115-event-processing)
    - [11.16 Distributed processes](#1116-distributed-processes)
    - [11.17 Domain Driven Design](#1117-domain-driven-design)
    - [11.18 Architecture Weekly](#1118-architecture-weekly)

## 1. Support

Feel free to [create an issue](https://github.com/oskardudycz/EventSourcing.NetCore/issues/new) if you have any questions or request for more explanation or samples. I also take **Pull Requests**!

💖 If this repository helped you - I'd be more than happy if you **join** the group of **my official supporters** at:

👉 [Github Sponsors](https://github.com/sponsors/oskardudycz) 

## 2. Prerequisites

For running the Event Store examples you need to have:

1. .NET 5 installed - https://dotnet.microsoft.com/download/dotnet/5.0
2. Postgres DB. You can get it by:
-   Installing [Docker](https://store.docker.com/search?type=edition&offering=community), going to the `docker` folder and running:

```
docker-compose up
```
-   Installing a most recent version of the Postgres DB (eg. from <https://www.postgresql.org/download/>).

**More information about using .NET Core, WebApi and Docker you can find in my other tutorials:** [WebApi with .NET](https://github.com/oskardudycz/WebApiWith.NETCore)


Watch "Practical Event Sourcing with Marten":

<a href="https://www.youtube.com/watch?v=L_ized5xwww&list=PLw-VZz_H4iio9b_NrH25gPKjr2MAS2YgC&index=7" target="_blank"><img src="https://img.youtube.com/vi/L_ized5xwww/0.jpg" alt="Practical Event Sourcing with Marten (EN)" width="320" height="240" border="10" /></a>

Slides:
- **Practical Event Sourcing with Marten** - [EN](./Slides/EventSourcing_with_Marten_EN.pptx), [PL](./Slides/EventSourcing_with_Marten_PL.pptx)
- **Ligths and Shades of Event-Driven Design** -  [EN](./Slides/Lights_And_Shades_Of_Event-Driven_Design.pptx), [PL](./Slides/SegFault-Blaski_i_Cienie.pptx)
- **Adventures in Event Sourcing and CQRS** - [PL](./Slides/Slides.pptx)

## 3. Libraries used

1. [Marten](https://github.com/JasperFx/marten) - Event Store

2. [MediatR](https://github.com/jbogard/MediatR) - Message Bus (for processing Commands, Queries, Events)

## 4. Articles
Read also more on the **Event Sourcing** and **CQRS** topics in my [blog](https://event-driven.io/?utm_source=event_sourcing_net) posts:
-   📝 [What's the difference between a command and an event?](https://event-driven.io/en/whats_the_difference_between_event_and_command/?utm_source=event_sourcing_net)
-   📝 [Events should be as small as possible, right?](https://event-driven.io/en/whats_the_difference_between_event_and_command/?utm_source=event_sourcing_net)
-   📝 [Why a bank account is not the best example of Event Sourcing?](https://event-driven.io/en/bank_account_event_sourcing/?utm_source=event_sourcing_net)
-   📝 [How to (not) do the events versioning?](https://event-driven.io/en/how_to_do_event_versioning/?utm_source=event_sourcing_net)
-   📝 [CQRS facts and myths explained](https://event-driven.io/en/cqrs_facts_and_myths_explained/?utm_source=event_sourcing_net)
-   📝 [Can command return a value?](https://event-driven.io/en/can_command_return_a_value/?utm_source=event_sourcing_net)
-   📝 [How to create projections of events for nested object structures?](https://event-driven.io/en/how_to_create_projections_of_events_for_nested_object_structures/?utm_source=event_sourcing_net)
-   📝 [What texting your Ex has to do with Event-Driven Design?](https://event-driven.io/en/what_texting_ex_has_to_do_with_event_driven_design/?utm_source=event_sourcing_net)
-   📝 [What if I told you that Relational Databases are in fact Event Stores?](https://event-driven.io/en/relational_databases_are_event_stores/?utm_source=event_sourcing_net)
-   📝 [Optimistic concurrency for pessimistic times](https://event-driven.io/en/optimistic_concurrency_for_pessimistic_times/?utm_source=event_sourcing_net)
-   📝 [Outbox, Inbox patterns and delivery guarantees explained](https://event-driven.io/en/outbox_inbox_patterns_and_delivery_guarantees_explained/?utm_source=event_sourcing_net)
-   📝 [Saga and Process Manager - distributed processes in practice](https://event-driven.io/en/saga_process_manager_distributed_transactions/?utm_source=event_sourcing_net)

## 5. Event Store - Marten

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
-   **Event stream aggregation** - events that were stored can be aggregated to form the entity once again. During the aggregation, process events are taken by the stream id and then replied event by event (so eg. NewTaskAdded, DescriptionOfTaskChanged, TaskRemoved). At first, an empty entity instance is being created (by calling default constructor). Then events based on the order of appearance (timestamp) are being applied on the entity instance by calling proper Apply methods.
    -   **[Aggregation general rules](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Marten.Integration.Tests/EventStore/Aggregate/AggregationRules.cs)**
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

## 6. Message Bus (for processing Commands, Queries, Events) - MediatR

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

## 7. CQRS (Command Query Responsibility Separation)

-   **[Command handling](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/CQRS.Tests/Commands/Commands.cs)**
-   **[Query handling](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/CQRS.Tests/Queries/Queries.cs)**

## 8. Fully working sample application

See also fully working sample application in [Sample Project](https://github.com/oskardudycz/EventSourcing.NetCore/tree/main/Sample)

-   See [sample](https://github.com/oskardudycz/EventSourcing.NetCore/tree/main/Sample/EventSourcing.Sample.IntegrationTests/Clients/CreateClientTests.cs) how Entity Framework and Marten can coexist together with CQRS and Event Sourcing

## 9. Self-paced training Kit

I prepared the self-paced training Kit for the Event Sourcing. See more in the [Workshop description](./Workshops/BuildYourOwnEventStore/Readme.md).

It's split into two parts:

**Event Sourcing basics** - it teaches the event store basics by showing how to build your Event Store on Relational Database. It starts with the tables setup, goes through appending events, aggregations, projections, snapshots, and finishes with the `Marten` basics. See more in [here](./Workshops/BuildYourOwnEventStore/01-EventStoreBasics/).

1. [Streams Table](./Workshops/BuildYourOwnEventStore/01-EventStoreBasics/01-CreateStreamsTable)
2. [Events Table](./Workshops/BuildYourOwnEventStore/01-EventStoreBasics/02-CreateEventsTable)
3. [Appending Events](./Workshops/BuildYourOwnEventStore/01-EventStoreBasics/03-CreateAppendEventFunction)
4. [Optimistic Concurrency Handling](./Workshops/BuildYourOwnEventStore/01-EventStoreBasics/03-OptimisticConcurrency)
5. [Event Store Methods](./Workshops/BuildYourOwnEventStore/01-EventStoreBasics/04-EventStoreMethods)
6. [Stream Aggregation](./Workshops/BuildYourOwnEventStore/01-EventStoreBasics/05-StreamAggregation)
7. [Time Travelling](./Workshops/BuildYourOwnEventStore/01-EventStoreBasics/06-TimeTraveling)
8. [Aggregate and Repositories](./Workshops/BuildYourOwnEventStore/01-EventStoreBasics/07-AggregateAndRepository)
9. [Snapshots](./Workshops/BuildYourOwnEventStore/01-EventStoreBasics/08-Snapshots)
10. [Projections](./Workshops/BuildYourOwnEventStore/01-EventStoreBasics/09-Projections)
11. [Projections With Marten](./Workshops/BuildYourOwnEventStore/01-EventStoreBasics/10-ProjectionsWithMarten)

**Event Sourcing advanced topics** - it's a real-world sample of the microservices written in Event-Driven design. It explains the topics of modularity, eventual consistency. Shows practical usage of WebApi, Marten as Event Store, Kafka as Event bus and ElasticSearch as one of the read stores. See more in [here](./Workshops/BuildYourOwnEventStore/02-EventSourcingAdvanced/).

1. [Meetings Management Module](./Workshops/BuildYourOwnEventStore/02-EventSourcingAdvanced/MeetingsManagement) - the module responsible for creating, updating meeting details. Written in `Marten` in **Event Sourcing** pattern. Provides both write model (with Event Sourced aggregates) and read model with projections.
2. [Meetings Search Module](./Workshops/BuildYourOwnEventStore/02-EventSourcingAdvanced/MeetingsSearch) - responsible for searching and advanced filtering. Uses `ElasticSearch` as storage (because of its advanced searching capabilities). It's a read module that's listening for the events published by the Meetings Management Module.

## 10. NuGet packages to help you get started.

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

## 11. Other resources

### 11.1 Introduction
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
-   🎞 [Thomas Bøgh Fangel - Event Sourcing: Traceability, Consistency, Correctness](https://www.youtube.com/watch?v=58_Ehl3oETY)
-   📝 [Joseph Choe - Event Sourcing, Part 1: User Registration](https://josephchoe.com/event-sourcing-part-1)
-   🎞 [Steven Van Beelen - Intro to Event-Driven Microservices using DDD, CQRS & Event sourcing](https://www.youtube.com/watch?v=F0g5B4F9MMs)
-   📝 [Yves Lorphelin - The Inevitable Event-Centric Book ](https://github.com/ylorph/The-Inevitable-Event-Centric-Book/issues)

### 11.2 Event Sourcing on production
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

### 11.3 Projections 
-   📝 [Alexey Zimarev - Projections in Event Sourcing](https://zimarev.com/blog/event-sourcing/projections/)
-   📝 [Rinat Abdulin - Event Sourcing - Projections](https://abdullin.com/post/event-sourcing-projections/)
-   🎞 [Derek Comartin - Projections in Event Sourcing: Build ANY model you want!](https://www.youtube.com/watch?v=bTRjO6JK4Ws)

### 11.4 Snapshots
-   📝 [Kacper Gunia - Event Sourcing: Snapshotting](https://domaincentric.net/blog/event-sourcing-snapshotting)
-   🎞 [Derek Comartin - Event Sourcing: Rehydrating Aggregates with Snapshots](https://www.youtube.com/watch?v=eAIkomEid1Y)

### 11.5 Versioning
-   📝 [Greg Young - Versioning in an Event Sourced System](https://leanpub.com/esversioning/read)
-   📝 [Kacper Gunia - Event Sourcing: Snapshotting](https://domaincentric.net/blog/event-sourcing-snapshotting)
-   📝 [M. Overeem, M. Spoor - The dark side of event sourcing: Managing data conversion](https://www.researchgate.net/publication/315637858_The_dark_side_of_event_sourcing_Managing_data_conversion)
-   📝 [Savvas Kleanthous - Event immutability and dealing with change](https://www.eventstore.com/blog/event-immutability-and-dealing-with-change)
-   📝 [Versioning in an Event Sourced System](http://blog.approache.com/2019/02/versioning-in-event-sourced-system-tldr_10.html?m=1)

### 11.6 Storage
-   📝 [Greg Young - Building an Event Storage](https://cqrs.wordpress.com/documents/building-event-storage/)
-   🎞 [Andrii Litvinov - Event driven systems backed by MongoDB](https://www.youtube.com/watch?v=w8Z-kPz1cXw)
-   📝 [Dave Remy - Turning the database inside out with Event Store](https://www.eventstore.com/blog/turning-the-database-inside-out)

### 11.7 Design & Modeling
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

### 11.8 GDPR
-   📝 [Michiel Rook - Event sourcing and the GDPR: a follow-up](https://www.michielrook.nl/2017/11/event-sourcing-gdpr-follow-up/)

### 11.9 Conflict Detection
-   🎞 [James Geall - Conflict Detection and Resolution in an EventSourced System](https://www.youtube.com/watch?v=-zaa6FUYIQM)
-   🎞 [Lightbend - Data modelling for Replicated Event Sourcing](https://www.youtube.com/watch?v=8PnJxTlOP6o)
-   📰 [Bartosz Sypytkowski - Collaborative Event Sourcing](https://www.slideshare.net/BartoszSypytkowski1/collaborative-replication)

### 11.10 Functional programming
-   📝 [Jérémie Chassaing - Functional Programming and Event Sourcing](https://www.youtube.com/watch?v=kgYGMVDHQHs)

### 11.12 Testing
-   🎞 [N. Rauch & A. Bailly - From Front to Back: Homomorphic Event Sourcing](https://www.youtube.com/watch?v=KyOvBQ87aP4)

### 11.13 CQRS
-   📝 [Greg Young - CQRS](https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf)
-   📝 [Jimmy Bogard - CQRS and REST: the perfect match](https://lostechies.com/jimmybogard/2016/06/01/cqrs-and-rest-the-perfect-match/)
-   📝 [Mark Seemann - CQS versus server-generated IDs](http://blog.ploeh.dk/2014/08/11/cqs-versus-server-generated-ids/)
-   📝 [Julie Lerman - Data Points - CQRS and EF Data Models](https://msdn.microsoft.com/en-us/magazine/mt788619.aspx)
-   📝 [Marco Bürckel - Some thoughts on using CQRS without Event Sourcing](https://medium.com/@mbue/some-thoughts-on-using-cqrs-without-event-sourcing-938b878166a2)
-   📝 [Bertrand Meyer - Eiffel: a language for software engineering (CQRS introduced)](http://laser.inf.ethz.ch/2012/slides/Meyer/eiffel_laser_2012.pdf)

### 11.14 Tools
-   🛠️ [Marten - .NET Transactional Document DB and Event Store on PostgreSQL](https://eventuous.dev/)
-   🛠️ [EventStoreDB - The stream database built for Event Sourcing ](https://developers.eventstore.com/)
-   🛠️ [GoldenEye - The CQRS flavoured framework that will speed up your WebAPI and Microservices development ](https://eventuous.dev/)
-   🛠️ [Eventuous - Event Sourcing for .NET](https://eventuous.dev/)
-   🛠️ [SQLStreamStore - Stream Store library targeting RDBMS based implementations for .NET ](https://github.com/SQLStreamStore/SQLStreamStore)
-   🛠️ [Equinox - .NET Event Sourcing library with CosmosDB, EventStoreDB, SqlStreamStore and integration test backends](https://github.com/jet/equinox)

### 11.15 Event Sourcing vs Messaging
-   📝 [Kacper Gunia - EventStoreDB vs Kafka](https://domaincentric.net/blog/eventstoredb-vs-kafka)
-   📝 [Vijay Nair - Axon and Kafka - How does Axon compare to Apache Kafka?](https://axoniq.io/blog-overview/axon-and-kafka-how-does-axon-compare-to-apache-kafka)

### 11.15 Event processing
-   📝 [Kamil Grzybek - The Outbox Pattern](http://www.kamilgrzybek.com/design/the-outbox-pattern/)
-   🎞 [Dev Mentors - Inbox & Outbox pattern - transactional message processing](https://www.youtube.com/watch?v=ebyR5RPKciw)
-   📝 [Jeremy D. Miller - Jasper's "Outbox" Pattern Support](https://jeremydmiller.com/2018/04/16/jaspers-outbox-pattern-support/)
-   📝 [Gunnar Morling  - Reliable Microservices Data Exchange With the Outbox Pattern](https://debezium.io/blog/2019/02/19/reliable-microservices-data-exchange-with-the-outbox-pattern/)
-   📝 [Microsoft - Asynchronous message-based communication](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/asynchronous-message-based-communication)
-   📝 [NServiceBus - Outbox](https://docs.particular.net/nservicebus/outbox/)
-   📝 [Alvaro Herrera - Implement SKIP LOCKED for row-level locks](https://www.depesz.com/2014/10/10/waiting-for-9-5-implement-skip-locked-for-row-level-locks/)

### 11.16 Distributed processes
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

### 11.17 Domain Driven Design
-   📝 [Eric Evans - DDD and Microservices: At Last, Some Boundaries!](https://www.infoq.com/presentations/ddd-microservices-2016)
-   📖 [Domain-Driven Design: The First 15 Years](https://leanpub.com/ddd_first_15_years/)
-   🎞 [Jimmy Bogard - Domain-Driven Design: The Good Parts](https://www.youtube.com/watch?v=U6CeaA-Phqo)
-   💻 [Jakub Pilimon - DDD by Examples](https://github.com/ddd-by-examples/library)
-   📖 [DDD Quickly](https://www.infoq.com/minibooks/domain-driven-design-quickly)
-   📝 [Vaughn Vernon - Reactive DDD: Modeling Uncertainty](https://www.infoq.com/presentations/reactive-ddd-distributed-systems)

### 11.18 Architecture Weekly
If you're interested in Architecture resources, check my other repository: https://github.com/oskardudycz/ArchitectureWeekly/.

It contains a weekly updated list of materials I found valuable and educational.

---

**EventSourcing.NetCore** is Copyright &copy; 2017-2021 [Oskar Dudycz](http://oskar-dudycz.pl) and other contributors under the [MIT license](LICENSE).