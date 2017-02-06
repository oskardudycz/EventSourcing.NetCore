# EventSourcing.NetCore
Example of Event Sourcing in .NET Core

#Prerequisites
Install recent version of the Postgres DB (eg. from link: https://www.postgresql.org/download/)

##Libraries used
1. [Marten](https://github.com/JasperFx/marten) - Event Store
2. [MediatR](https://github.com/jbogard/MediatR) - Message Bus (eg. for Commands, Queries, Events)

##Suggested Order of reading
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

2. MediatR - Message Bus (eg. for Commands, Queries, Events)
 * [Initialization](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Initialization/Initialization.cs) - MediatR uses services locator pattern to find proper handler for message type
 * [Sending Messages] - Sending messages finds and uses first registered handler for the message type
  * [No Handlers](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Sending/NoHandlers.cs)
  * [Synchronous Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Sending/SynchronousHandler.cs)
  * [Aynchronous Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Sending/AsynchronousHandler.cs)
  * [More Than One Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Sending/MoreThanOneHandler.cs)
 * [Publishing Messages] - Publishing messages finds and uses all registered handlers for the message type
  * [No Handlers](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/NoHandlers.cs)
  * [Synchronous Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/SynchronousHandler.cs)
  * [Aynchronous Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/AsynchronousHandler.cs)
  * [More Than One Handler](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/MediatR.Tests/Publishing/MoreThanOneHandler.cs)
