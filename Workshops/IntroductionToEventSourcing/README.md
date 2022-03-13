# Introduction to Event Sourcing Workshop

## Exercises

1. [Events definition](./01-EventsDefinition).
2. [Getting State from events](./02-GettingStateFromEvents).
3. Appending Events:
   * [Marten](./03-AppendingEvents.Marten)
   * [EventStoreDB](./04-AppendingEvents.EventStoreDB)
4. Getting State from events
   * [Marten](./05-GettingStateFromEvents.Marten)
   * [EventStoreDB](./06-GettingStateFromEvents.EventStoreDB)
5. Business logic:
   * [General](./07-BusinessLogic)
   * [Marten](./08-BusinessLogic.Marten)
   * [EventStoreDB](./09-BusinessLogic.EventStoreDB)
6. Optimistic Concurrency:
   * [Marten](./10-OptimisticConcurrency.Marten)
   * [EventStoreDB](./11-OptimisticConcurrency.EventStoreDB)
7. Projections:
   * [General](./12-Projections)
   * [Idempotency](./13-Projections.Idempotency)
   * [Eventual Consistency](./14-Projections.EventualConsistency)

## Prerequisites

1. Install git - https://git-scm.com/downloads.
2. Clone this repository.
3. Install .NET 6.0 - https://dotnet.microsoft.com/download/dotnet/6.0.
4. Install Visual Studio 2022, Rider or VSCode.
5. Install docker - https://docs.docker.com/engine/install/.
6. Open [Exercises.sln](./Exercises.sln) solution.

## Running

1. Run: `docker-compose up`.
2. Wait until all dockers got are downloaded and running.
3. You should automatically get:
    - Postgres DB running for [Marten storage](https://martendb.io)
    - PG Admin - IDE for postgres. Available at: http://localhost:5050.
        - Login: `admin@pgadmin.org`, Password: `admin`
        - To connect to server click right mouse on Servers, then Register Server and use host: `postgres`, user: `postgres`, password: `Password12!`
   - EventStoreDB UI: http://localhost:2113/
4. Open, build and run [Exercises.sln](./Exercises.sln) solution.
