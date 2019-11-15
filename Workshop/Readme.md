# Event Sourcing Self-Paced Kit

## Prerequisities

1. Install git - https://git-scm.com/downloads.
2. Install .NET Core 2.2 - https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.2.108-windows-x64-installer.
3. Install Visual Studio (suggested 2017) or Rider.
4. Install docker - https://docs.docker.com/docker-for-windows/install/.
5. Make sure that you have ~10GB disk space.
6. Create Github Account
7. Clone Project https://github.com/oskardudycz/EventSourcing.NetCore, make sure that's compiling
8. Go to gitter channel https://gitter.im/oskardudycz/szkola-event-sourcing.
9. Check https://github.com/StackExchange/Dapper/, https://github.com/jbogard/MediatR, http://jasperfx.github.io/marten/documentation/
10. Go to [docker folder](../docker/) open CMD and run `docker-compose up`. Other useful commands are:

    - `docker-compose kill` to stop running dockers.
    - `docker-compose down -v` to clean stopped dockers.
    - `docker ps` - for showing running dockers
    - `docker ps -a` - to show all dockers (also stopped)

11. Wait until all dockers got are downloaded and running.
12. You should automatically get:

    - Postgres DB running
    - PG Admin - IDE for postgres. Available at: http://localhost:5050.
        - Login: `pgadmin4@pgadmin.org`, Password: `admin`
        - To connect to server Use host: `postgres`, user: `postgres`, password: `Password12!`
    - Kafka
    - Kafka ide for browsing topics. Available at: http://localhost:8000
    - ElasticSearch
    - Kibana for browsing ElasticSearch - http://localhost:5601

I prepared self-paced training Kit for the Event Sourcing. See more in the [Workshop description](./Workshop/Readme.md).

## Description

It's splitted into two parts:

**Event Sourcing basics** - it teaches the event store basics by showing how to build your own Event Store on Relational Database. It starts with the tables setup, goes through appending events, aggregations, projectsions, snapshots and finishes with the `Marten` basics. See more in [here](./Workshop/01-EventStoreBasics/).

1. [Streams Table](./Workshop/01-EventStoreBasics/01-CreateStreamsTable)
2. [Events Table](./Workshop/01-EventStoreBasics/02-CreateEventsTable)
3. [Appending Events](./Workshop/01-EventStoreBasics/03-CreateAppendEventFunction)
4. [Optimistic Concurrency Handling](03-OptimisticConcurrency)
5. [Event Store Methods](./Workshop/01-EventStoreBasics/04-EventStoreMethods)
6. [Stream Aggregation](./Workshop/01-EventStoreBasics/05-StreamAggregation)
7. [Time Travelling](./Workshop/01-EventStoreBasics/06-TimeTraveling)
8. [Aggregate and Repositories](./Workshop/01-EventStoreBasics/07-AggregateAndRepository)
9. [Snapshots](./Workshop/01-EventStoreBasics/08-Snapshots)
10. [Projections](./Workshop/01-EventStoreBasics/09-Projections)
11. [Projections With Marten](./Workshop/01-EventStoreBasics/10-ProjectionsWithMarten)

**Event Sourcing advanced topics** - it's a real world sample of the microservices written in Event-Driven design. It explains the topics of modularity, eventual consistency. Shows practical usage of WebApi, Marten as Event Store, Kafka as Event bus and ElasticSearch as one of the read stores. See more in [here](./Workshop/02-EventSourcingAdvanced/).

1. [Meetings Management Module](./Workshop/02-EventSourcingAdvanced/MeetingsManagement) - module responsible for creating, updating meetings details. Written in `Marten` in **Event Sourcing** pattern. Provides both write model (with Event Sourced aggregates) and read model with projections.
2. [Meetings Search Module](./Workshop/02-EventSourcingAdvanced/MeetingsSearch) - responsible for searching and advanced filtering. Uses `ElasticSearch` as a storage (because of it's advanced searching capabilities). It's a read module that's listening for the events published by Meetings Management Module.
