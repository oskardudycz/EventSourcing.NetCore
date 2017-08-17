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
    * **[Stream loading](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Stream/StreamLoading.cs)** - all events that were placed on the event store should be possible to load them back. Marten allows to:
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
    * **[Reaggregation](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Aggregate/Reaggregation.cs)** - one of the biggest advantage of the event sourcing is flexibility to business logic updates. It's not needed to perform complex miggration. For online aggregation it's not needed to perform reaggregation - it's being made always automatically. Inline aggregation needs to be reaggregated. It can be done by performing online aggregation on all stream events and storing the result as a snapshot.
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
    * **[Synchronous Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Sending/SynchronousHandler.cs)** - by implementing implementing IRequestHandler we're making decision that this handler should be run one by one with other synchronous handlers.
    * **[Aynchronous Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Sending/AsynchronousHandler.cs)** - by implementing implementing IAsyncRequestHandler we're making decision that this handler should be run asynchronously with other async handlers (so we don't wait for the previous handler to finish its work).
    * **[More Than One Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Sending/MoreThanOneHandler.cs)** - when there is more than one handler registered MediatR takes only one ignoring others when Send method is being called.
  * **Publishing Messages** - finds and uses all registered handlers for the message type. It's good for processing events.
    * **[No Handlers](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/NoHandlers.cs)** - when MediatR doesn't find proper handler it throws an exception
    * **[Synchronous Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/SynchronousHandler.cs)** - by implementing implementing INotificationHandler we're making decision that this handler should be run one by one with other synchronous handlers.
    * **[Aynchronous Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/AsynchronousHandler.cs)** - by implementing implementing IAsyncNotificationHandler we're making decision that this handler should be run asynchronously with other async handlers (so we don't wait for the previous handler to finish its work)
    * **[More Than One Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/MoreThanOneHandler.cs)** - when there is more than one handler registered MediatR takes only all of them when calling Publish method
  * Pipeline (to be defined)
  
### 3. CQRS (Command Query Responsibility Separation)
  * **[Command handling](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/CQRS.Tests/Commands/Commands.cs)**
  * **[Query handling](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/CQRS.Tests/Queries/Queries.cs)**
