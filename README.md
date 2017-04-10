# EventSourcing.NetCore
Example of Event Sourcing in .NET Core

## Prerequisites
Install recent version of the Postgres DB (eg. from link: https://www.postgresql.org/download/)

Video presentation (PL): https://www.youtube.com/watch?v=i1XDr9km0RY  
Slides (PL): https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Slides.pptx

## Libraries used
1. [Marten](https://github.com/JasperFx/marten) - Event Store

2. [MediatR](https://github.com/jbogard/MediatR) - Message Bus (for processing Commands, Queries, Events)

## Suggested Order of reading
1. Marten Event Store
  * [Creating event store](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/General/StoreInitializationTests.cs)
  * Event Stream
    * [Stream starting](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Stream/StreamStarting.cs)
    * [Stream loading](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Stream/StreamLoading.cs)
  * Event stream aggregation
    * [Aggregation general rules](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Aggregate/AggregationRules.cs)
    * [Online Aggregation](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Aggregate/EventsAggregation.cs)
    * [Inline Aggregation](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Aggregate/InlineAggregationStorage.cs)
  * Event transformations
    * [One event to one object transformations](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Transformations/OneToOneEventTransformations.cs)
    * [Inline Transformation storage](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Transformations/InlineTransformationsStorage.cs)

2. MediatR - Message Bus (for processing Commands, Queries, Events)
  * [Initialization](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Initialization/Initialization.cs) - MediatR uses services locator pattern to find proper handler for message type.
  * Sending Messages - finds and uses first registered handler for the message type. It could be used for queries (when we need to return values), commands (when we performing an action).
    * [No Handlers](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Sending/NoHandlers.cs) - when MediatR doesn't find proper handler it throws an exception.
    * [Synchronous Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Sending/SynchronousHandler.cs) - by implementing implementing IRequestHandler we're making decision that this handler should be run one by one with other synchronous handlers.
    * [Aynchronous Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Sending/AsynchronousHandler.cs) - by implementing implementing IAsyncRequestHandler we're making decision that this handler should be run asynchronously with other async handlers (so we don't wait for the previous handler to finish its work).
    * [More Than One Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Sending/MoreThanOneHandler.cs) - when there is more than one handler registered MediatR takes only one ignoring others when Send method is being called.
  * Publishing Messages - finds and uses all registered handlers for the message type. It's good for processing events.
    * [No Handlers](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/NoHandlers.cs) - when MediatR doesn't find proper handler it throws an exception
    * [Synchronous Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/SynchronousHandler.cs) - by implementing implementing INotificationHandler we're making decision that this handler should be run one by one with other synchronous handlers.
    * [Aynchronous Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/AsynchronousHandler.cs) - by implementing implementing IAsyncNotificationHandler we're making decision that this handler should be run asynchronously with other async handlers (so we don't wait for the previous handler to finish its work)
    * [More Than One Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/MoreThanOneHandler.cs) - when there is more than one handler registered MediatR takes only all of them when calling Publish method
  * Pipeline (to be defined)
  
3. CQRS (Command Query Responsibility Separation)
  * [Command handling](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/CQRS.Tests/Commands/Commands.cs)
  * [Query handling](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/CQRS.Tests/Queries/Queries.cs)
