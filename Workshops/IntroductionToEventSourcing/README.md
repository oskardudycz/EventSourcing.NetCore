[<img src="https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" height="20px" />](https://www.linkedin.com/in/oskardudycz/) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_jvm) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net) 

# Introduction to Event Sourcing Workshop

Event Sourcing is perceived as a complex pattern. Some believe that it's like Nessie, everyone's heard about it, but rarely seen it. In fact, Event Sourcing is a pretty practical and straightforward concept. It helps build predictable applications closer to business. Nowadays, storage is cheap, and information is priceless. In Event Sourcing, no data is lost. 

The workshop aims to build the knowledge of the general concept and its related patterns for the participants. The acquired knowledge will allow for the conscious design of architectural solutions and the analysis of associated risks. 

The emphasis will be on a pragmatic understanding of architectures and applying it in practice using Marten and KurrentDB.

1. Introduction to Event-Driven Architectures. Differences from the classical approach are foundations and terminology (event, event streams, command, query).
2. What is Event Sourcing, and how is it different from Event Streaming. Advantages and disadvantages.
3. Write model, data consistency guarantees on examples from Marten and KurrentDB.
4. Various ways of handling business logic: Aggregates, Command Handlers, functional approach.
5. Projections, best practices and concerns for building read models from events on the examples from Marten and KurrentDB.
6. Challenges in Event Sourcing and EDA: deliverability guarantees, sequence of event handling, idempotency, etc.
8. Saga, Choreography, Process Manager,  distributed processes in practice.
7. Event Sourcing in the context of application architecture, integration with other approaches (CQRS, microservices, messaging, etc.).
8. Good and bad practices in event modelling.
9. Event Sourcing on production, evolution, events' schema versioning, etc.

You can do the workshop as a self-paced kit. That should give you a good foundation for starting your journey with Event Sourcing and learning tools like Marten and KurrentDB. If you'd like to get full coverage with all nuances of the private workshop, feel free to contact me via [email](mailto:oskar.dudycz@gmail.com).

Read also more in my article [Introduction to Event Sourcing - Self Paced Kit](https://event-driven.io/en/introduction_to_event_sourcing/?utm_source=event_sourcing_net).

## Exercises

1. [Events definition](./01-EventsDefinition).
2. [Getting State from events](./02-GettingStateFromEvents).
3. Appending Events:
    * [Marten](./03-AppendingEvents.Marten)
    * [KurrentDB](./04-AppendingEvents.KurrentDB)
4. Getting State from events
    * [Marten](./05-GettingStateFromEvents.Marten)
    * [KurrentDB](./06-GettingStateFromEvents.KurrentDB)
5. Business logic:
    * [General](./07-BusinessLogic)
    * [Slimmed](./07-BusinessLogic.Slimmed)
6. Application Logic:
    * [Marten](./08-ApplicationLogic.Marten)
    * [KurrentDB](./09-ApplicationLogic.KurrentDB)
7. Optimistic Concurrency:
    * [Marten](./10-OptimisticConcurrency.Marten)
    * [KurrentDB](./11-OptimisticConcurrency.KurrentDB)
8. Projections:
    * [General](./12-Projections.SingleStream)
    * [Idempotency](./13-Projections.SingleStream.Idempotency)
    * [Eventual Consistency](./14-Projections.SingleStream.EventualConsistency)
9. Multi-Stream Projections:
    * [General](./15-Projections.MultiStream)
    * [Out-of-Order Events](./16-Projections.MultiStream.OutOfOrder)
    * [Marten](./17-Projections.MultiStream.Marten)
    * [Out-of-Order Events with Marten](./18-Projections.MultiStream.OutOfOrder.Marten)
10. Business Processes:
    * [Events definition](./19-EventsDefinition)
    * [Entities definition](./20-EntitiesDefinition)
    * [Business processes](./21-BusinessProcesses)
    * [Idempotency](./22-Idempotency)
    * [Consistency](./23-Consistency)

## Prerequisites

1. Install git - https://git-scm.com/downloads.
2. Clone this repository.
3. Install .NET 10.0 - https://dotnet.microsoft.com/download/dotnet/10.0.
4. Install Visual Studio, Rider or VSCode.
5. Install docker - https://docs.docker.com/engine/install/.
6. Open [Exercises.slnx](./Exercises.slnx) solution.

## Running

1. Run: `docker compose --profile all up` (or for Mac `docker compose -f docker-compose.arm.yml --profile all up`). Tests use these containers. If they aren't running, tests start their own through [Testcontainers](https://dotnet.testcontainers.org/), so Docker running is enough.
2. Wait until all dockers got are downloaded and running.
3. You should automatically get:
    - Postgres DB running for [Marten storage](https://martendb.io)
    - PG Admin - IDE for postgres. Available at: http://localhost:5050.
        - Login: `admin@pgadmin.org`, Password: `admin`
        - To connect to server click right mouse on Servers, then Register Server and use host: `postgres`, user: `postgres`, password: `postgres`
   - KurrentDB UI: http://localhost:2113/
4. Open, build and run [Exercises.slnx](./Exercises.slnx) solution.
4. To see solved exercises, open [Solved.slnx](./Solved.slnx) solution.
