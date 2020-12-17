![Twitter Follow](https://img.shields.io/twitter/follow/oskar_at_net?style=social) [![Join the chat at https://gitter.im/EventSourcing-NetCore/community](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/EventSourcing-NetCore/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) [![Build status](https://ci.appveyor.com/api/projects/status/8gb320jrp40el9ye/branch/main?svg=true)](https://ci.appveyor.com/project/oskardudycz/eventsourcing-netcore/branch/main) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/)

# EventSourcing.NetCore

Tutorial, practical samples and other resources about Event Sourcing in .NET Core.

- [EventSourcing.NetCore](#eventsourcingnetcore)
  - [1. Support](#1-support)
  - [2. Prerequisites](#2-prerequisites)
  - [3. Libraries used](#3-libraries-used)
  - [4. Event Store - Marten](#4-event-store---marten)
  - [5. Message Bus (for processing Commands, Queries, Events) - MediatR](#5-message-bus-for-processing-commands-queries-events---mediatr)
  - [6. CQRS (Command Query Responsibility Separation)](#6-cqrs-command-query-responsibility-separation)
  - [7. Fully working sample application](#7-fully-working-sample-application)
  - [8. Self-paced training Kit](#8-self-paced-training-kit)
  - [9. NuGet packages to help you get started.](#9-nuget-packages-to-help-you-get-started)
  - [10. Other resources](#10-other-resources)
    - [10.1 Various](#101-various)
    - [10.2 Snapshots](#102-snapshots)
    - [10.3 Projections](#103-projections)
    - [10.4 Event processing](#104-event-processing)
    - [10.5 Distributed processes](#105-distributed-processes)
    - [10.6 Modeling](#106-modeling)
  - [11. Contributors](#11-contributors)
    - [11.1 Code Contributors](#111-code-contributors)
    - [11.2 Financial Contributors](#112-financial-contributors)
      - [Individuals](#individuals)
      - [Organizations](#organizations)

## 1. Support

Feel free to [create an issue](https://github.com/oskardudycz/EventSourcing.NetCore/issues/new) if you have any questions or request for more explanation or samples. I also take **Pull Requests**!

💖 If this repository helped you - I'd be more than happy if you **join** the group of **my official supporters** at:

👉 [Github Sponsors](https://github.com/sponsors/oskardudycz) 

## 2. Prerequisites

For running the Event Store examples you need to have Postgres DB. You can get it by:

-   Installing [Docker](https://store.docker.com/search?type=edition&offering=community), going to the `docker` folder and running:

```
docker-compose up
```

**More information about using .NET Core, WebApi and Docker you can find in my other tutorial:** [.Net Core With Docker](https://github.com/oskardudycz/NetCoreWithDocker)

-   Installing a most recent version of the Postgres DB (eg. from <https://www.postgresql.org/download/>).

Video presentation:

<a href="https://www.youtube.com/watch?v=L_ized5xwww&list=PLw-VZz_H4iio9b_NrH25gPKjr2MAS2YgC&index=7" target="_blank"><img src="https://img.youtube.com/vi/L_ized5xwww/0.jpg" alt="Practical Event Sourcing with Marten (EN)" width="320" height="240" border="10" /></a>

Slides:
- **Practical Event Sourcing with Marten** - [EN](./Slides/EventSourcing_with_Marten_EN.pptx), [PL](./Slides/EventSourcing_with_Marten_PL.pptx)
- **Ligths and Shades of Event-Driven Design** -  [EN](./Slides/Lights_And_Shades_Of_Event-Driven_Design.pptx), [PL](./Slides/SegFault-Blaski_i_Cienie.pptx)
- **Adventures in Event Sourcing and CQRS** - [PL](./Slides/Slides.pptx)

## 3. Libraries used

1. [Marten](https://github.com/JasperFx/marten) - Event Store

2. [MediatR](https://github.com/jbogard/MediatR) - Message Bus (for processing Commands, Queries, Events)

## 4. Event Store - Marten

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
    -   **[Projection of single stream](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Marten.Integration.Tests/EventStore/Projections/ViewProjectionsTest.cs)**
-   **[Multitenancy per schema](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Marten.Integration.Tests/Tenancy/TenancyPerSchema.cs)**

## 5. Message Bus (for processing Commands, Queries, Events) - MediatR

-   **[Initialization](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/MediatR.Tests/Initialization/Initialization.cs)** - MediatR uses services locator pattern to find a proper handler for the message type.
-   **Sending Messages** - finds and uses the first registered handler for the message type. It could be used for queries (when we need to return values), commands (when we acting).
    -   **[No Handlers](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/MediatR.Tests/Sending/NoHandlers.cs)** - when MediatR doesn't find proper handler it throws an exception.
    -   **[Single Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/MediatR.Tests/Sending/SingleHandler.cs)** - by implementing IRequestHandler we're deciding that this handler should be run asynchronously with other async handlers (so we don't wait for the previous handler to finish its work).
    -   **[More Than One Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/MediatR.Tests/Sending/MoreThanOneHandler.cs)** - when there is more than one handler registered MediatR takes only one ignoring others when Send method is being called.
-   **Publishing Messages** - finds and uses all registered handlers for the message type. It's good for processing events.
    -   **[No Handlers](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/MediatR.Tests/Publishing/NoHandlers.cs)** - when MediatR doesn't find proper handler it throws an exception
    -   **[Single Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/MediatR.Tests/Publishing/SingleHandler.cs)** - by implementing INotificationHandler we're deciding that this handler should be run asynchronously with other async handlers (so we don't wait for the previous handler to finish its work)
    -   **[More Than One Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/MediatR.Tests/Publishing/MoreThanOneHandler.cs)** - when there is more than one handler registered MediatR takes only all of them when calling Publish method
-   Pipeline (to be defined)

## 6. CQRS (Command Query Responsibility Separation)

-   **[Command handling](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/CQRS.Tests/Commands/Commands.cs)**
-   **[Query handling](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/CQRS.Tests/Queries/Queries.cs)**

## 7. Fully working sample application

See also fully working sample application in [Sample Project](https://github.com/oskardudycz/EventSourcing.NetCore/tree/main/Sample)

-   See [sample](https://github.com/oskardudycz/EventSourcing.NetCore/tree/main/Sample/EventSourcing.Sample.IntegrationTests/Clients/CreateClientTests.cs) how Entity Framework and Marten can coexist together with CQRS and Event Sourcing

## 8. Self-paced training Kit

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

## 9. NuGet packages to help you get started.

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

## 10. Other resources

### 10.1 Various
-   📝 [Greg Young - Building an Event Storage](https://cqrs.wordpress.com/documents/building-event-storage/)
-   📝 [Mathias Verraes - DDD and Messaging Architectures](http://verraes.net/2019/05/ddd-msg-arch/)
-   📝 [Microsoft - Exploring CQRS and Event Sourcing](https://docs.microsoft.com/en-us/previous-versions/msp-n-p/jj554200(v=pandp.10))
-   🎞 [Greg Young - CQRS & Event Sourcing](https://youtube.com/watch?v=JHGkaShoyNs)
-   📰 [Lorenzo Nicora - A visual introduction to event sourcing and cqrs](https://www.slideshare.net/LorenzoNicora/a-visual-introduction-to-event-sourcing-and-cqrs)
-   🎞 [Greg Young - A Decade of DDD, CQRS, Event Sourcing](https://m.youtube.com/watch?v=LDW0QWie21s)
-   🎞 [Martin Fowler - The Many Meanings of Event-Driven Architecture](https://www.youtube.com/watch?v=STKCRSUsyP0&t=822s)
-   📝 [Martin Fowler - Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
-   📝 [Wojciech Suwała - Building Microservices On .NET Core – Part 5 Marten An Ideal Repository For Your Domain Aggregates](https://altkomsoftware.pl/en/blog/building-microservices-domain-aggregates/)
-   🎞 [Dennis Doomen - A practical introduction to DDD, CQRS & Event Sourcing](https://www.youtube.com/watch?v=r26BuahD8aM)
-   📝 [Dennis Doomen - 16 design guidelines for successful Event Sourcing](https://www.continuousimprover.com/2020/06/guidelines-event-sourcing.html)
-   📝 [Eric Evans - DDD and Microservices: At Last, Some Boundaries!](https://www.infoq.com/presentations/ddd-microservices-2016)
-   🎞 [Martin Kleppmann - Event Sourcing and Stream Processing at Scale](https://www.youtube.com/watch?v=avi-TZI9t2I)
-   📝 [Julie Lerman - Data Points - CQRS and EF Data Models](https://msdn.microsoft.com/en-us/magazine/mt788619.aspx)
-   📝 [Vaughn Vernon - Reactive DDD: Modeling Uncertainty](https://www.infoq.com/presentations/reactive-ddd-distributed-systems)
-   📝 [Mark Seemann - CQS versus server-generated IDs](http://blog.ploeh.dk/2014/08/11/cqs-versus-server-generated-ids/)
-   📦 [Event Store - The open-source, functional database with Complex Event Processing in JavaScript](https://eventstore.org/)
-   📝 [Pedro Costa - Migrating to Microservices and Event-Sourcing: the Dos and Dont’s](https://hackernoon.com/migrating-to-microservices-and-event-sourcing-the-dos-and-donts-195153c7487d)
-   📝 [David Boike - Putting your events on a diet](https://particular.net/blog/putting-your-events-on-a-diet)
-   📖 [DDD Quickly](https://www.infoq.com/minibooks/domain-driven-design-quickly)
-   📝 [Dennis Doomen - The Good, The Bad and the Ugly of Event Sourcing](https://www.continuousimprover.com/2017/11/event-sourcing-good-bad-and-ugly.html)
-   📰 [Dennis Doomen - Practical introduction to DDD, CQRS and Event Sourcing ](https://www.slideshare.net/dennisdoomen/practical-introduction-to-ddd-cqrs-and-event-sourcing)
-   📝 [Versioning in an Event Sourced System](http://blog.approache.com/2019/02/versioning-in-event-sourced-system-tldr_10.html?m=1)
-   📰 [Bartosz Sypytkowski - Collaborative Event Sourcing](https://www.slideshare.net/BartoszSypytkowski1/collaborative-replication)
-   📝 [Jay Kreps - Why local state is a fundamental primitive in stream processing](https://www.oreilly.com/ideas/why-local-state-is-a-fundamental-primitive-in-stream-processing)
-   🎞 [Thomas Pierrain - As Time Goes By… (a Bi-temporal Event Sourcing story)](https://youtube.com/watch?v=xzekp1RuZbM)
-   📝 [Michiel Overeem, Marten Spoor, Slinger Jansen - The dark side of event sourcing: Managing data conversion](https://www.researchgate.net/publication/315637858_The_dark_side_of_event_sourcing_Managing_data_conversion)
-   💻 [Jakub Pilimon - DDD by Examples](https://github.com/ddd-by-examples/library)
-   🎞 [Michiel Overeem - Event Sourcing after launch](https://www.youtube.com/watch?v=JzWJI8kW2kc)
-   🎞 [Jimmy Bogard - Domain-Driven Design: The Good Parts](https://www.youtube.com/watch?v=U6CeaA-Phqo)
-   📝 [Jimmy Bogard - CQRS and REST: the perfect match](https://lostechies.com/jimmybogard/2016/06/01/cqrs-and-rest-the-perfect-match/)
-   📝 [Microsoft - Domain events: design and implementation](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/domain-events-design-implementation)
-   🎞 [Mathew McLoughlin - An Introduction to CQRS and Event Sourcing Patterns](https://www.youtube.com/watch?v=9a1PqwFrMP0)
-   🎞 [Alexey Zimarev - DDD, Event Sourcing and Actors](https://www.youtube.com/watch?v=58_Ehl3oETY)

### 10.2 Snapshots
-   📝 [Kacper Gunia - Event Sourcing: Snapshotting](https://domaincentric.net/blog/event-sourcing-snapshotting)
-   📝 [Event Store - Rolling Snapshots](https://eventstore.com/docs/event-sourcing-basics/rolling-snapshots/index.html)

### 10.3 Projections 
-   📝 [Alexey Zimarev - Projections in Event Sourcing](https://zimarev.com/blog/event-sourcing/projections/)
-   📝 [Rinat Abdulin - Event Sourcing - Projections](https://abdullin.com/post/event-sourcing-projections/)

### 10.4 Event processing
-   📝 [Kamil Grzybek - The Outbox Pattern](http://www.kamilgrzybek.com/design/the-outbox-pattern/)
-   🎞 [Dev Mentors - Inbox & Outbox pattern - transactional message processing](https://www.youtube.com/watch?v=ebyR5RPKciw)
-   📝 [Jeremy D. Miller - Jasper’s “Outbox” Pattern Support](https://jeremydmiller.com/2018/04/16/jaspers-outbox-pattern-support/)
-   📝 [Gunnar Morling  - Reliable Microservices Data Exchange With the Outbox Pattern](https://debezium.io/blog/2019/02/19/reliable-microservices-data-exchange-with-the-outbox-pattern/)
-   📝 [Michrosoft - Asynchronous message-based communication](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/asynchronous-message-based-communication)
-   📝 [NServiceBus - Outbox](https://docs.particular.net/nservicebus/outbox/)
-   📝 [Alvaro Herrera - Implement SKIP LOCKED for row-level locks](https://www.depesz.com/2014/10/10/waiting-for-9-5-implement-skip-locked-for-row-level-locks/)

### 10.5 Distributed processes
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

### 10.6 Modeling
-   📝 [Event Modeling](https://eventmodeling.org/posts/what-is-event-modeling/)
-   📝 [Event Storming](https://leanpub.com/introducing_eventstorming)
-   📝 [Vaughn Vernon - Effective Aggregate Design Part I: Modeling a Single Aggregate](https://dddcommunity.org/wp-content/uploads/files/pdf_articles/Vernon_2011_1.pdf)

## 11. Contributors

### 11.1 Code Contributors

This project exists thanks to all the people who contribute. [[Contribute](CONTRIBUTING.md)].
<a href="https://github.com/oskardudycz/EventSourcing.NetCore/graphs/contributors"><img src="https://opencollective.com/eventsourcingnetcore/contributors.svg?width=890&button=false" /></a>

### 11.2 Financial Contributors

Become a financial contributor and help us sustain our community. [[Contribute](https://opencollective.com/eventsourcingnetcore/contribute)]

#### Individuals

<a href="https://opencollective.com/eventsourcingnetcore"><img src="https://opencollective.com/eventsourcingnetcore/individuals.svg?width=890"></a>

#### Organizations

Support this project with your organization. Your logo will show up here with a link to your website. [[Contribute](https://opencollective.com/eventsourcingnetcore/contribute)]

<a href="https://opencollective.com/eventsourcingnetcore/organization/0/website"><img src="https://opencollective.com/eventsourcingnetcore/organization/0/avatar.svg"></a>
<a href="https://opencollective.com/eventsourcingnetcore/organization/1/website"><img src="https://opencollective.com/eventsourcingnetcore/organization/1/avatar.svg"></a>
<a href="https://opencollective.com/eventsourcingnetcore/organization/2/website"><img src="https://opencollective.com/eventsourcingnetcore/organization/2/avatar.svg"></a>
<a href="https://opencollective.com/eventsourcingnetcore/organization/3/website"><img src="https://opencollective.com/eventsourcingnetcore/organization/3/avatar.svg"></a>
<a href="https://opencollective.com/eventsourcingnetcore/organization/4/website"><img src="https://opencollective.com/eventsourcingnetcore/organization/4/avatar.svg"></a>
<a href="https://opencollective.com/eventsourcingnetcore/organization/5/website"><img src="https://opencollective.com/eventsourcingnetcore/organization/5/avatar.svg"></a>
<a href="https://opencollective.com/eventsourcingnetcore/organization/6/website"><img src="https://opencollective.com/eventsourcingnetcore/organization/6/avatar.svg"></a>
<a href="https://opencollective.com/eventsourcingnetcore/organization/7/website"><img src="https://opencollective.com/eventsourcingnetcore/organization/7/avatar.svg"></a>
<a href="https://opencollective.com/eventsourcingnetcore/organization/8/website"><img src="https://opencollective.com/eventsourcingnetcore/organization/8/avatar.svg"></a>
<a href="https://opencollective.com/eventsourcingnetcore/organization/9/website"><img src="https://opencollective.com/eventsourcingnetcore/organization/9/avatar.svg"></a>

**EventSourcing.NetCore** is Copyright &copy; 2017-2020 [Oskar Dudycz](http://oskar-dudycz.pl) and other contributors under the [MIT license](LICENSE).
