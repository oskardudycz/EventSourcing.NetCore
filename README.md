# EventSourcing.NetCore
Example of Event Sourcing in .NET Core

#Prerequisites
Install recent version of the Postgres DB (eg. from link: https://www.postgresql.org/download/)

##Libraries used
1. [Marten](https://github.com/JasperFx/marten) - Event Store
2. [MediatR](https://github.com/jbogard/MediatR) - Event Bus

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
