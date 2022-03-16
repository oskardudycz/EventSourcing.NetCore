# Introduction to Event Sourcing Workshop

Event Sourcing is perceived as a complex pattern. Some believe that it's like Nessie, everyone's heard about it, but rarely seen it. In fact, Event Sourcing is a pretty practical and straightforward concept. It helps build predictable applications closer to business. Nowadays, storage is cheap, and information is priceless. In Event Sourcing, no data is lost. 

The workshop aims to build the knowledge of the general concept and its related patterns for the participants. The acquired knowledge will allow for the conscious design of architectural solutions and the analysis of associated risks. 

The emphasis will be on a pragmatic understanding of architectures and applying it in practice using Marten and EventStoreDB.

1. Introduction to Event-Driven Architectures. Differences from the classical approach are foundations and terminology (event, event streams, command, query).
2. What is Event Sourcing, and how is it different from Event Streaming. Advantages and disadvantages.
3. Write model, data consistency guarantees on examples from Marten and EventStoreDB.
4. Various ways of handling business logic: Aggregates, Command Handlers, functional approach.
5. Projections, best practices and concerns for building read models from events on the examples from Marten and EventStoreDB.
6. Challenges in Event Sourcing and EDA: deliverability guarantees, sequence of event handling, idempotency, etc.
8. Saga, Choreography, Process Manager,  distributed processes in practice.
7. Event Sourcing in the context of application architecture, integration with other approaches (CQRS, microservices, messaging, etc.).
8. Good and bad practices in event modelling.
9. Event Sourcing on production, evolution, events' schema versioning, etc.

You can do the workshop as a self-paced kit. That should give you a good foundation for starting your journey with Event Sourcing and learning tools like Marten and EventStoreDB. If you'd like to get full coverage with all nuances of the private workshop, feel free to contact me via [email](mailto:oskar.dudycz@gmail.com).

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
4. To see solved exercises, open [Solved.sln](./Solved.sln) solution.
