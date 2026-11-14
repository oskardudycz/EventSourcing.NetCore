[<img src="https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" height="20px" />](https://www.linkedin.com/in/oskardudycz/) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_jvm) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net) 

# E-Commerce sample of Event Sourcing with KurrentDB

Sample is showing the typical flow of the Event Sourcing app with [KurrentDB](https://docs.kurrent.io/).

## Prerequisites

1. Install git - https://git-scm.com/downloads.
2. Install .NET 10.0 - https://dotnet.microsoft.com/download/dotnet/10.0.
3. Install Visual Studio 2022, Rider or VSCode.
4. Install docker - https://docs.docker.com/engine/install/.
5. Open [EventSourcing.NetCore.slnx](../../../EventSourcing.NetCore.slnx) solution.

## Running

1. Go to [docker](./docker) and run: `docker compose up`.
2. Wait until all containers are downloaded and running.
3. You should automatically get:
    - KurrentDB UI (for event store): http://localhost:2113/
    - Postgres DB running (for read models)
    - PG Admin - IDE for postgres. Available at: http://localhost:5050.
        - Login: `pgadmin4@pgadmin.org`, Password: `admin`
        - To connect to server Use host: `postgres`, user: `postgres`, password: `Password12!`
4. Build and run the `Carts.Api` project.
    - Swagger should be available at: http://localhost:5000/index.html


## Overview

It uses:
- CQRS with command and query handlers registered in the DI container,
- Stores events from Aggregate method results to KurrentDB,
- Builds read models using [Subscription to `$all`](https://docs.kurrent.io/clients/dotnet/v1.4/subscriptions#basic-subscriptions).
- Read models are stored as [Marten](https://martendb.io/) documents.
- App has Swagger and predefined [docker-compose](./docker/docker-compose.yml) to run and play with samples.

## Write Model

- Most of the write model infrastructure was reused from other samples,
- Added new project `Core.KurrentDB` for specific KurrentDB code,
- Added [KurrentDBRepository](../../../Core.KurrentDB/Repository/KurrentDBRepository.cs) repository to load and store aggregate state,
- Added separate [IProjection](../../../Core/Projections/IProjection.cs) interface to handle the same way stream aggregation and materialised projections,
- Thanks to that added dedicated [AggregateStream](../../../Core.KurrentDB/Events/AggregateStreamExtensions.cs#L10) method for stream aggregation
- See [sample Aggregate](./Carts/Carts/ShoppingCarts/ShoppingCart.cs)

## Read Model
- Read models are rebuilt with eventual consistency using subscribe to all KurrentDB feature,
- Uses hosted service [KurrentDBSubscriptionToAll](../../../Core.KurrentDB/Subscriptions/KurrentDBSubscriptionToAll.cs) to handle subscribing to all. It handles checkpointing and simple retries if the connection was dropped.
- Uses checkpointing to KurrentDB stream with [KurrentDBSubscriptionCheckpointRepository](../../../Core.KurrentDB/Subscriptions/Checkpoints/KurrentDBSubscriptionCheckpointRepository.cs) and dummy in-memory checkpointer [InMemorySubscriptionCheckpointRepository](../../../Core.KurrentDB/Subscriptions/Checkpoints/InMemorySubscriptionCheckpointRepository.cs),
- Uses [MartenExternalProjection](../../../Core.Marten/ExternalProjections/MartenExternalProjection.cs) as a sample how to project with [`left-fold`](https://en.wikipedia.org/wiki/Fold_(higher-order_function)) into external storage. Another (e.g. ElasticSearch, EntityFramework) can be implemented the same way.

## Tests
- Added sample of unit testing in [`Carts.Tests`](./Carts/Carts.Tests):
    - [Aggregate unit tests](./Carts/Carts.Tests/Carts/OpeningCart/OpenCartTests.cs)
    - [Command handler unit tests](./Carts/Carts.Tests/Carts/OpeningCart/OpenCartCommandHandlerTests.cs)
- Added sample of integration testing in [`Carts.Api.Tests`](./Carts/Carts.Api.Tests)
    - [API integration tests](./Carts/Carts.Api.Tests/ShoppingCarts/Opening/OpenShoppingCartTests.cs)

## Other
- Uses [EventTypeMapper](../../../Core/Events/EventTypeMapper.cs) class to allow both convention-based mapping (by the .NET type name) and custom to handle event versioning,
- Uses [StreamNameMapper](../../../Core/Events/StreamNameMapper.cs) class for convention-based id (and optional tenant) mapping based on the stream type and module,
- IoC [registration helpers for KurrentDB configuration](../../../Core.KurrentDB/Config.cs),


## Trivia

1. Docker useful commands
    - `docker compose up` - start dockers
    - `docker compose kill` - to stop running dockers.
    - `docker compose down -v` - to clean stopped dockers.
    - `docker ps` - for showing running dockers
    - `docker ps -a` - to show all dockers (also stopped)

