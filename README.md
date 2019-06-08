[![Build status](https://ci.appveyor.com/api/projects/status/8gb320jrp40el9ye/branch/master?svg=true)](https://ci.appveyor.com/project/oskardudycz/eventsourcing-netcore/branch/master) ![](https://dockerbuildbadges.quelltext.eu/status.svg?organization=oskardudycz&repository=eventsourcing.netcore)

# EventSourcing.NetCore
Example of Event Sourcing in .NET Core

## Prerequisites

For running the Event Store examples you need to have Postgres DB. You can get it by:
* Installing [Docker](https://store.docker.com/search?type=edition&offering=community), going to the `docker` folder and running:
```
docker-compose up
```

**More information about using .NET Core, WebApi and Docker you can find in my other tutorial:** [.Net Core With Docker](https://github.com/oskardudycz/NetCoreWithDocker)

* Installing most recent version of the Postgres DB (eg. from: <https://www.postgresql.org/download/>). 



Video presentation (PL): 

<a href="https://www.youtube.com/watch?feature=player_embedded&v=i1XDr9km0RY" target="_blank"><img src="https://img.youtube.com/vi/i1XDr9km0RY/0.jpg" alt="Video presentation (PL)" width="320" height="240" border="10" /></a>

Slides (PL):  
<https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Slides.pptx>

## Libraries used
1. [Marten](https://github.com/JasperFx/marten) - Event Store

2. [MediatR](https://github.com/jbogard/MediatR) - Message Bus (for processing Commands, Queries, Events)

## Suggested Order of reading
### 1. Event Store - Marten
  * **[Creating event store](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/General/StoreInitializationTests.cs)**
  * **Event Stream** - is a representation of the entity in event sourcing. It's a set of events that hapened for the entity with the exact id. Stream id should be unique, can have different types but usually is a Guid.
    * **[Stream starting](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Stream/StreamStarting.cs)** - stream should be always started with a unique id. Marten provides three ways of starting stream:  
      * calling StartStream method with a stream id  
         ```csharp
         var streamId = Guid.NewGuid();
         documentSession.Events.StartStream<TaskList>(streamId);
         ```  
      * calling StartStream method with a set of events  
         ```csharp
         var @event = new TaskCreated { TaskId = Guid.NewGuid(), Description = "Description" };
         var streamId = documentSession.Events.StartStream<TaskList>(@event);
         ```  
      * just appending events with a stream id
         ```csharp
         var @event = new TaskCreated { TaskId = Guid.NewGuid(), Description = "Description" };
         var streamId = Guid.NewGuid();
         documentSession.Events.Append(streamId, @event);
         ```  
    * **[Stream loading](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Stream/StreamLoading.cs)** - all events that were placed on the event store should be possible to load them back. [Marten](https://github.com/JasperFx/marten) allows to:
      * get list of event by calling FetchStream method with a stream id  
         ```csharp
         var eventsList = documentSession.Events.FetchStream(streamId);
         ```  
      * geting one event by its id  
         ```csharp
         var @event = documentSession.Events.Load<TaskCreated>(eventId);
         ```  
    * **[Stream loading from exact state](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Stream/StreamLoadingFromExactState.cs)** - all events that were placed on the event store should be possible to load them back. Marten allows to get stream from exact state by:
      * timestamp (has to be in UTC)
         ```csharp
         var dateTime = new DateTime(2017, 1, 11);
         var events = documentSession.Events.FetchStream(streamId, timestamp: dateTime);
         ```  
      * version number
         ```csharp
         var versionNumber = 3;
         var events = documentSession.Events.FetchStream(streamId, version: versionNumber);
         ```  
  * **Event stream aggregation** - events that were stored can be aggregated to form the entity once again. During aggregation process events are taken by the stream id and then replied event by event (so eg. NewTaskAdded, DescriptionOfTaskChanged, TaskRemoved). At first empty entity instance is being created (by calling default constructor). Then events based of the order of apperance (timestamp) are being applied on the entity instance by calling proper Apply methods.
    * **[Aggregation general rules](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Aggregate/AggregationRules.cs)**
    * **[Online Aggregation](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Aggregate/EventsAggregation.cs)** - online aggregation is a process when entity instance is being constructed on the fly from events. Events are taken from the database and then aggregation is being done. The biggest advantage of the online aggregation is that it always gets the most recent business logic. So after the change it's automatically reflected and it's not needed to do any migration or updates.
    * **[Inline Aggregation (Snapshot)](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Aggregate/InlineAggregationStorage.cs)** - inline aggregation happens when we take the snapshot of the entity from the db. In that case it's not needed to get all events. Marten stores the snapshot as a document. This is good for the performance reasons, because only one record is being materialized. The con of using inline aggregation is that after business logic has changed records need to be reaggregated.
    * **[Reaggregation](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Aggregate/Reaggregation.cs)** - one of the biggest advantage of the event sourcing is flexibility to business logic updates. It's not needed to perform complex migration. For online aggregation it's not needed to perform reaggregation - it's being made always automatically. Inline aggregation needs to be reaggregated. It can be done by performing online aggregation on all stream events and storing the result as a snapshot.
      * reaggregation of inline snapshot with Marten
         ```csharp
         var onlineAggregation = documentSession.Events.AggregateStream<TEntity>(streamId);
         documentSession.Store<TEntity>(onlineAggregation);
         documentSession.SaveChanges();
         ```  
  * **Event transformations**
    * **[One event to one object transformations](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Transformations/OneToOneEventTransformations.cs)**
    * **[Inline Transformation storage](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Transformations/InlineTransformationsStorage.cs)**
  * **Events projection**
    * **[Projection of single stream](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Projections/ViewProjectionsTest.cs)**

### 2. Message Bus (for processing Commands, Queries, Events) - MediatR
  * **[Initialization](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Initialization/Initialization.cs)** - MediatR uses services locator pattern to find proper handler for message type.
  * **Sending Messages** - finds and uses first registered handler for the message type. It could be used for queries (when we need to return values), commands (when we performing an action).
    * **[No Handlers](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Sending/NoHandlers.cs)** - when MediatR doesn't find proper handler it throws an exception.
    * **[Single Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Sending/SingleHandler.cs)** - by implementing implementing IRequestHandler we're making decision that this handler should be run asynchronously with other async handlers (so we don't wait for the previous handler to finish its work).
    * **[More Than One Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Sending/MoreThanOneHandler.cs)** - when there is more than one handler registered MediatR takes only one ignoring others when Send method is being called.
  * **Publishing Messages** - finds and uses all registered handlers for the message type. It's good for processing events.
    * **[No Handlers](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/NoHandlers.cs)** - when MediatR doesn't find proper handler it throws an exception
    * **[Single Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/SingleHandler.cs)** - by implementing implementing INotificationHandler we're making decision that this handler should be run asynchronously with other async handlers (so we don't wait for the previous handler to finish its work)
    * **[More Than One Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/MoreThanOneHandler.cs)** - when there is more than one handler registered MediatR takes only all of them when calling Publish method
  * Pipeline (to be defined)
  
### 3. CQRS (Command Query Responsibility Separation)
  * **[Command handling](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/CQRS.Tests/Commands/Commands.cs)**
  * **[Query handling](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/CQRS.Tests/Queries/Queries.cs)**

### 4. Fully working sample application
See also fully working sample application in [Sample Project](https://github.com/oskardudycz/EventSourcing.NetCore/tree/master/Sample)
* See [sample](https://github.com/oskardudycz/EventSourcing.NetCore/tree/master/Sample/EventSourcing.Sample.IntegrationTests/Clients/CreateClientTests.cs) how Entity Framework and Marten can coexist together with CQRS and Event Sourcing


### 5. Nuget packages to help you get started.
I gathered and generalized all of practices used in this tutorial/samples in Nuget Packages of maintained by me [GoldenEye Framework](https://github.com/oskardudycz/GoldenEye).
See more in:
  * [GoldenEye DDD package](https://github.com/oskardudycz/GoldenEye/tree/master/src/Core/Backend.Core.DDD) - it provides set of base and bootstrap classes that helps you to reduce boilerplate code and help you focus on writing business code. You can find all classes like Commands/Queries/Event handlers and many more. To use it run:

  `dotnet add package GoldenEye.Backend.Core.DDD`
  * [GoldenEye Marten package](https://github.com/oskardudycz/GoldenEye/tree/master/src/Core/Backend.Core.Marten) - contains helpers, and abstractions to use Marten as document/event store. Gives you abstractions like repositories etc. To use it run:

  `dotnet add package GoldenEye.Backend.Core.Marten`


The simplest way to start is **installing the [project template](https://github.com/oskardudycz/GoldenEye/tree/master/src/Templates/SimpleDDD/content) by running**

`dotnet new -i GoldenEye.WebApi.Template.SimpleDDD`

**and then creating new project based on it:**

`dotnet new SimpleDDD -n NameOfYourProject`


### 6. Other resources

* [Greg Young - Building an Event Storage](https://cqrs.wordpress.com/documents/building-event-storage/)
* [Microsoft - Exploring CQRS and Event Sourcing](https://docs.microsoft.com/en-us/previous-versions/msp-n-p/jj554200(v=pandp.10))
* [Greg Young - CQRS & Event Sourcing](https://m.youtube.com/watch?v=JHGkaShoyNs)
* [Lorenzo Nicora - A visual introduction to event sourcing and cqrs](https://www.slideshare.net/LorenzoNicora/a-visual-introduction-to-event-sourcing-and-cqrs)
* [Greg Young - A Decade of DDD, CQRS, Event Sourcing](https://m.youtube.com/watch?v=LDW0QWie21s)
* [Martin Fowler - Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
* [Wojciech Suwała - Building Microservices On .NET Core – Part 5 Marten An Ideal Repository For Your Domain Aggregates](https://altkomsoftware.pl/en/blog/building-microservices-domain-aggregates/)
* [Eric Evans - DDD and Microservices: At Last, Some Boundaries!](https://www.infoq.com/presentations/ddd-microservices-2016)
* [Martin Kleppmann - Event Sourcing and Stream Processing at Scale](https://www.youtube.com/watch?v=avi-TZI9t2I)
* [Thomas Pierrain - As Time Goes By… (a Bi-temporal Event Sourcing story)](https://www.youtube.com/watch?v=xzekp1RuZbM)
* [Julie Lerman - Data Points - CQRS and EF Data Models](https://msdn.microsoft.com/en-us/magazine/mt788619.aspx)
* [Vaughn Vernon - Reactive DDD: Modeling Uncertainty](https://www.infoq.com/presentations/reactive-ddd-distributed-systems)
* [Mark Seemann - CQS versus server generated IDs](http://blog.ploeh.dk/2014/08/11/cqs-versus-server-generated-ids/)
* [Udi Dahan - If (domain logic) then CQRS, or Saga?](https://www.youtube.com/watch?v=fWU8ZK0Dmxs&app=desktop)
* [Event Store - The open-source, functional database with Complex Event Processing in JavaScript](https://eventstore.org/)
* [Pedro Costa - Migrating to Microservices and Event-Sourcing: the Dos and Dont’s](https://hackernoon.com/migrating-to-microservices-and-event-sourcing-the-dos-and-donts-195153c7487d)
* [David Boike - Putting your events on a diet](https://particular.net/blog/putting-your-events-on-a-diet)
* [DDD Quickly](https://www.infoq.com/minibooks/domain-driven-design-quickly)
* [Dennis Doomen - The Good, The Bad and the Ugly of Event Sourcing](https://www.continuousimprover.com/2017/11/event-sourcing-good-bad-and-ugly.html)
* [Liquid Projections - A set of highly efficient building blocks to build fast autonomous synchronous and asynchronous projector](https://liquidprojections.net/)
* [Versioning in an Event Sourced System](http://blog.approache.com/2019/02/versioning-in-event-sourced-system-tldr_10.html?m=1)
* [Jay Kreps - Why local state is a fundamental primitive in stream processing](https://www.oreilly.com/ideas/why-local-state-is-a-fundamental-primitive-in-stream-processing)
* [Thomas Pierrain - As Time Goes By… (a Bi-temporal Event Sourcing story)](https://m.youtube.com/watch?v=xzekp1RuZbM)
* [Michiel Overeem, Marten Spoor, Slinger Jansen - The dark side of event sourcing: Managing data conversion](https://www.researchgate.net/publication/315637858_The_dark_side_of_event_sourcing_Managing_data_conversion)
* [Jakub Pilimon - DDD by Examples](https://github.com/ddd-by-examples/library)

I found an issue or I have a change request
--------------------------------
Feel free to create an issue on GitHub. Contributions, pull requests are more than welcome!

**EventSourcing.NetCore** is Copyright &copy; 2017-2019 [Oskar Dudycz](http://oskar-dudycz.pl) and other contributors under the [MIT license](LICENSE).
