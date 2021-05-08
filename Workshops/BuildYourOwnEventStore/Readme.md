# Build Your Own Event Store Self-Paced Kit

## Prerequisities

1. Install git - https://git-scm.com/downloads.
2. Install .NET 5 - https://dotnet.microsoft.com/download/dotnet/5.0.
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

12. For the first part of workshop please go to [./docker](./docker) and run: `docker-compose up`.
13. Wait until all dockers got are downloaded and running.
14. You should automatically get:

    - Postgres DB running
    - PG Admin - IDE for postgres. Available at: http://localhost:5050.
        - Login: `admin@pgadmin.org`, Password: `admin`
        - To connect to server Use host: `postgres`, user: `postgres`, password: `Password12!`

## Description

**Event Sourcing basics** - it teaches the event store basics by showing how to build your own Event Store on Relational Database. It starts with the tables setup, goes through appending events, aggregations, projectsions, snapshots and finishes with the `Marten` basics. See more in [here](./01-EventStoreBasics/).

1. [Streams Table](./01-CreateStreamsTable)
2. [Events Table](./02-CreateEventsTable)
3. [Appending Events](./03-CreateAppendEventFunction)
4. [Optimistic Concurrency Handling](./03-OptimisticConcurrency)
5. [Event Store Methods](./04-EventStoreMethods)
6. [Stream Aggregation](./05-StreamAggregation)
7. [Time Travelling](./06-TimeTraveling)
8. [Aggregate and Repositories](./07-AggregateAndRepository)
9. [Snapshots](./08-Snapshots)
10. [Projections](./09-Projections)
11. [Projections With Marten](./10-ProjectionsWithMarten)
