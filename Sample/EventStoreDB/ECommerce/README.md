# E-Commerce sample of Event Sourcing with EventStoreDB

Sample is showing the typical flow of the Event Sourcing app with [EventStoreDB](https://developers.eventstore.com).

## Prerequisities

1. Install git - https://git-scm.com/downloads.
2. Install .NET Core 5.0 - https://dotnet.microsoft.com/download/dotnet/5.0.
3. Install Visual Studio 2019, Rider or VSCode.
4. Install docker - https://docs.docker.com/docker-for-windows/install/.
5. Open `ECommerce.sln` solution.

## Running

1. Go to [docker](./docker) and run: `docker-compose up`.
2. Wait until all dockers got are downloaded and running.
3. You should automatically get:
    - Postgres DB running
    - PG Admin - IDE for postgres. Available at: http://localhost:5050.
        - Login: `admin@pgadmin.org`, Password: `admin`
        - To connect to server Use host: `postgres`, user: `postgres`, password: `Password12!`
    - EventStoreDB UI: http://localhost:2113/
4. Open, build and run `ECommerce.sln` solution.
	- Swagger should be available at: http://localhost:5000/index.html


## Overview

It uses:
- CQRS with MediatR,
- Stores events from Aggregate method results to EventStoreDB,
- Builds read models using [Subscription to `$all`](https://developers.eventstore.com/clients/grpc/subscribing-to-streams/#subscribing-to-all).
- Read models are stored as [Marten](https://martendb.io/) documents.
- App has Swagger and predefined [docker-compose](https://github.com/oskardudycz/EventSourcing.NetCore/pull/49/files#diff-bd9579f0d00fbcbca25416ada9698a7f38fdd91b710c1651e0849d56843a6b45) to run and play with samples.

## Write Model

- Most of the write model infrastructure was reused from other samples,
- Added new project `Core.EventStoreDB` for specific EventStoreDB code,
- Added [EventStoreDBRepository](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Core.EventStoreDB/Repository/EventStoreDBRepository.cs) repository to load and store aggregate state,
- Added separate [IProjection](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Core/Projections/IProjection.cs) interface to handle the same way stream aggregation and materialised projections,
- Thanks to that added dedicated [AggregateStream](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Core.EventStoreDB/Events/AggregateStreamExtensions.cs#L12) method for stream aggregation

## Read Model
- Read models are rebuilt with eventual consistency using subscribe to all EventStoreDB feature,
- Added hosted service [SubscribeToAllBackgroundWorker](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Core.EventStoreDB/Subscriptions/SubscribeToAllBackgroundWorker.cs) to handle subscribing to all. It handles checkpointing and simple retries if the connection was dropped.
- Added [ISubscriptionCheckpointRepository](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Core/Subscriptions/ISubscriptionCheckpointRepository.cs) for handling Subscription checkpointing.
- Added checkpointing to EventStoreDB stream with [EventStoreDBSubscriptionCheckpointRepository](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Core.EventStoreDB/Subscriptions/EventStoreDBSubscriptionCheckpointRepository.cs) and dummy in-memory checkpointer [InMemorySubscriptionCheckpointRepository](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Core/Subscriptions/InMemorySubscriptionCheckpointRepository.cs),
- Added [MartenExternalProjection](https://github.com/oskardudycz/EventSourcing.NetCore/pull/49/files#diff-6d8dadf8ab81a9441836a5403632ef3616a1dc42788b5feae1c56a4f2321d4eeR12) as a sample how to project with [`left-fold`](https://en.wikipedia.org/wiki/Fold_(higher-order_function)) into external storage. Another (e.g. ElasticSearch, EntityFramework) can be implemented the same way.

## Tests
- Added sample of unit testing in `Carts.Tests`,
- Added sample of integration testing in `Carts.Tests.Api`

## Other
- Added [EventTypeMapper](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Core/Events/EventTypeMapper.cs) class to allow both convention-based mapping (by the .NET type name) and custom to handle event versioning,
- Added [StreamNameMapper](https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Core/Events/StreamNameMapper.cs) class for convention-based id (and optional tenant) mapping based on the stream type and module,
- IoC registration helpers - https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Core.EventStoreDB/Config.cs,


## Trivia

1. Docker useful commands
    - `docker-compose up` - start dockers
    - `docker-compose kill` - to stop running dockers.
    - `docker-compose down -v` - to clean stopped dockers.
    - `docker ps` - for showing running dockers
    - `docker ps -a` - to show all dockers (also stopped)

