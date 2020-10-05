# Event Sourcing Self-Paced Kit

## Prerequisities

1. Install git - https://git-scm.com/downloads.
2. Install .NET Core 3.1 - https://dotnet.microsoft.com/download/dotnet-core/thank-you/sdk-3.1.100-windows-x64-installer.
3. Install Visual Studio 2019, Rider or VSCode.
4. Install docker - https://docs.docker.com/docker-for-windows/install/.
5. Make sure that you have ~10GB disk space.
6. Create Github Account
7. Clone Project https://github.com/oskardudycz/EventSourcing.NetCore, make sure that's compiling
8. Go to gitter channel https://gitter.im/oskardudycz/szkola-event-sourcing.
9. Check https://github.com/StackExchange/Dapper/, https://github.com/jbogard/MediatR, http://jasperfx.github.io/marten/documentation/
10. Open `BuildYourOwnEventStore.sln` solution.
11. Docker useful commands

    - `docker-compose up` - start dockers
    - `docker-compose kill` - to stop running dockers.
    - `docker-compose down -v` - to clean stopped dockers.
    - `docker ps` - for showing running dockers
    - `docker ps -a` - to show all dockers (also stopped)

12. For the first part of workshop please go to [./01-EventStoreBasics/docker](./01-EventStoreBasics/docker) and run: `docker-compose up`.
13. Wait until all dockers got are downloaded and running.
14. You should automatically get:

    - Postgres DB running
    - PG Admin - IDE for postgres. Available at: http://localhost:5050.
        - Login: `admin@pgadmin.org`, Password: `admin`
        - To connect to server Use host: `postgres`, user: `postgres`, password: `Password12!`

15. Before the second part run `docker-compose kill` kill running docker images.
16. Then go to [./02-EventSourcingAdvanced/docker](./02-EventSourcingAdvanced/docker) and run: `docker-compose up`.
17. You should automatically get (besides `Postgres` and `PGAdmin`):
    - Kafka
    - Kafka ide for browsing topics. Available at: http://localhost:8000
    - ElasticSearch
    - Kibana for browsing ElasticSearch - http://localhost:5601

I prepared self-paced training Kit for the Event Sourcing. See more in the [Workshop description](./Readme.md).

## Description

It's splitted into two parts:

**Event Sourcing basics** - it teaches the event store basics by showing how to build your own Event Store on Relational Database. It starts with the tables setup, goes through appending events, aggregations, projectsions, snapshots and finishes with the `Marten` basics. See more in [here](./01-EventStoreBasics/).

1. [Streams Table](./01-EventStoreBasics/01-CreateStreamsTable)
2. [Events Table](./01-EventStoreBasics/02-CreateEventsTable)
3. [Appending Events](./01-EventStoreBasics/03-CreateAppendEventFunction)
4. [Optimistic Concurrency Handling](03-OptimisticConcurrency)
5. [Event Store Methods](./01-EventStoreBasics/04-EventStoreMethods)
6. [Stream Aggregation](./01-EventStoreBasics/05-StreamAggregation)
7. [Time Travelling](./01-EventStoreBasics/06-TimeTraveling)
8. [Aggregate and Repositories](./01-EventStoreBasics/07-AggregateAndRepository)
9. [Snapshots](./01-EventStoreBasics/08-Snapshots)
10. [Projections](./01-EventStoreBasics/09-Projections)
11. [Projections With Marten](./01-EventStoreBasics/10-ProjectionsWithMarten)

**Event Sourcing advanced topics** - it's a real world sample of the microservices written in Event-Driven design. It explains the topics of modularity, eventual consistency. Shows practical usage of WebApi, Marten as Event Store, Kafka as Event bus and ElasticSearch as one of the read stores. See more in [here](./02-EventSourcingAdvanced/).

1. [Meetings Management Module](./02-EventSourcingAdvanced/MeetingsManagement) - module responsible for creating, updating meetings details. Written in `Marten` in **Event Sourcing** pattern. Provides both write model (with Event Sourced aggregates) and read model with projections.
2. [Meetings Search Module](./02-EventSourcingAdvanced/MeetingsSearch) - responsible for searching and advanced filtering. Uses `ElasticSearch` as a storage (because of it's advanced searching capabilities). It's a read module that's listening for the events published by Meetings Management Module.
