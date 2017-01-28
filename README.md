# EventSourcing.NetCore
Example of Event Sourcing in .NET Core

#Prerequisites
Install recent version of the Postgres (eg. from link: https://www.postgresql.org/download/)

##Libraries used
1. [Marten](https://github.com/JasperFx/marten) - Event Store

##Suggested Order of learning
1. Marten Event Store
  * [Creating data store](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/General/StoreInitializationTests.cs)
  * Event Stream
    * [Stream starting](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Stream/StreamStarting.cs)
    * [Stream loading](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Stream/StreamLoading.cs)
  * Event stream aggregation
    * [Aggregation general rules](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Aggregate/AggregationRules.cs)
    * [Aggregation example](https://github.com/oskardudycz/EventSourcing.NetCore/blob/master/Marten.Integration.Tests/EventStore/Aggregate/EventsAggregation.cs)
