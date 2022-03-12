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

## Prerequisites

1. Install git - https://git-scm.com/downloads.
2. Install .NET Core 6.0 - https://dotnet.microsoft.com/download/dotnet/6.0.
3. Install Visual Studio 2022, Rider or VSCode.
4. Install docker - https://docs.docker.com/docker-for-windows/install/.
5. Open [Exercises.sln](./Exercises.sln) solution.

## Running

1. Run: `docker-compose up`.
2. Wait until all dockers got are downloaded and running.
3. You should automatically get:
    - Postgres DB running for [Marten storage](https://martendb.io)
    - PG Admin - IDE for postgres. Available at: http://localhost:5050.
        - Login: `admin@pgadmin.org`, Password: `admin`
        - To connect to server Use host: `postgres`, user: `postgres`, password: `Password12!
   - EventStoreDB UI: http://localhost:2113/`
4. Open, build and run [Exercises.sln](./Exercises.sln) solution.
