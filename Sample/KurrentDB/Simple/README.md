[<img src="https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" height="20px" />](https://www.linkedin.com/in/oskardudycz/) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_jvm) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net) 

# Simple, practical EventSourcing with KurrentDB and EntityFramework

This is the simple Event Sourcing setup with KurrentDB. For the Read Model, Postgres and Entity Framework are used.

You can watch the webinar on YouTube where I'm explaining the details of the implementation:

<a href="https://www.youtube.com/watch?v=rqYPVzjoxqI" target="_blank"><img src="https://img.youtube.com/vi/rqYPVzjoxqI/0.jpg" alt="Practical introduction to Event Sourcing with EventStoreDB" width="640" height="480" border="10" /></a>

or read the article explaining the read model part: ["How to build event-driven projections with Entity Framework"](https://event-driven.io/en/how_to_do_events_projections_with_entity_framework/)

## Main assumptions:
- explain basics of Event Sourcing, both from the write model (KurrentDB) and read model part (Postgres and EntityFramework),
- CQRS architecture sliced by business features, keeping code that changes together at the same place. Read more in [How to slice the codebase effectively?](https://event-driven.io/en/how_to_slice_the_codebase_effectively/)
- no aggregates, just data (records) and functions,
- clean, composable (pure) functions for command, events, projections, query handling instead of marker interfaces (the only one used internally is `IEventHandler`). Thanks to that testability and easier maintenance.
- easy to use and self-explanatory fluent API for registering commands and projections with possible fallbacks,
- registering everything into regular DI containers to integrate with other application services.
- pushing the type/signature enforcement on edge, so when plugging to DI.

## Overview

It uses:
- pure data entities, functions and handlers,
- Stores events from the command handler result in KurrentDB,
- Builds read models using [Subscription to `$all`](https://docs.kurrent.io/clients/dotnet/v1.4/subscriptions#basic-subscriptions).
- Read models are stored to Postgres relational tables with [Entity Framework](https://docs.microsoft.com/en-us/ef/core/).
- App has Swagger and predefined [docker-compose](./docker/docker-compose.yml) to run and play with samples.

## Write Model
- Sample [ShoppingCart](./ECommerce/ShoppingCarts/ShoppingCart.cs#L39) entity and [events](./ECommerce/ShoppingCarts/ShoppingCart.cs#L5) represent the business workflow. All are stored in the same file to be able to understand flow without jumping from one file to another. It also contains [Evolve](./ECommerce/ShoppingCarts/ShoppingCart.cs#L50) method defining how to apply events to get the entity state. It uses the C#9 switch syntax with records deconstruction.
- Example [ProductItemsList](./ECommerce/ShoppingCarts/ProductItems/ProductItemsList.cs) value object wrapping the list of product items in the shopping carts. It simplified the main state apply logic and offloaded some of the invariants checks.
- All commands by convention should be created using the [factory method](./ECommerce/ShoppingCarts/AddingProductItem/AddProductItemToShoppingCart.cs#L11) to enforce the types,
- Command handlers are defined as static methods in the same file as command definition. Usually, they change together. They are pure functions that take command and/or state and create new events based on the business logic. See sample [Adding Product Item to ShoppingCart](./ECommerce/ShoppingCarts/AddingProductItem/AddProductItemToShoppingCart.cs#L21). This example also shows that you can inject external services to handlers if needed.
- [Added syntax for self-documenting command handlers registration](./ECommerce/ShoppingCarts/Configuration.cs#L21). See the details of registration in [CommandHandlerExtensions](./ECommerce.Core/Commands/CommandHandler.cs). They differentiate case when [a new entity/stream is created](./ECommerce.Core/Commands/CommandHandler.cs#L10) from the [update case](./ECommerce.Core/Commands/CommandHandler.cs#L22). Update has to support optimistic concurrency. Added also [Command Handlers Builder](./ECommerce.Core/Commands/CommandHandler.cs#L131) for simplifying the registrations.
- Added simple [KurrentDB extensions](./ECommerce.Core/KurrentDB/KurrentDBExtensions.cs) repository to load entity state and store event created by business logic,

## Read Model
- Read models are rebuilt with eventual consistency using subscribe to $all stream KurrentDB feature,
- Used Entity Framework to store projection data into Postgres tables,
- Added sample projection for [Shopping cart details](./ECommerce/ShoppingCarts/GettingCartById/ShoppingCartDetails.cs) and slimmed [Shopping cart short info](./ECommerce/ShoppingCarts/GettingCarts/ShoppingCartShortInfo.cs) as an example of different interpretations of the same events. Shopping cart details also contain a nested collection of product items to show more advanced use case. All event handling is done by functions. It enables easier unit and integration testing.
- [Added syntax for self-documenting projection handlers registration](./ECommerce/ShoppingCarts/Configuration.cs#L52). See the details of registration in [EntityFrameworkProjectionBuilder](../../../Core.EntityFramework/Projections/EntityFrameworkProjectionBuilder.cs#L50). They differentiate case when [a new read model is created](./ECommerce.Core/Projections/EntityFrameworkProjection.cs#L13) from the [update case](./ECommerce.Core/Projections/EntityFrameworkProjection.cs#L39). Update has to support optimistic concurrency.
- [example query handlers](./ECommerce/ShoppingCarts/GettingCarts/GetCarts.cs#L20) for reading data together with [registration helpers](../../../Core.EntityFramework/Projections/EntityFrameworkProjectionBuilder.cs#L88) for EntityFramework querying.
- Used service [KurrentDBSubscriptionToAll](../../../Core.KurrentDB/Subscriptions/KurrentDBSubscriptionToAll.cs) to handle subscribing to all. It handles checkpointing and simple retries when the connection is dropped. Added also general [BackgroundWorker](../../../Core/BackgroundWorkers/BackgroundWorker.cs) to wrap the general `IHostedService` handling
- Used checkpointing to Postgres with [PostgresSubscriptionCheckpointRepository](../../../Core.KurrentDB/Subscriptions/Checkpoints/Postgres/PostgresSubscriptionCheckpointRepository.cs),
- Used custom [EventBus](../../../Core/Events/EventBus.cs) implementation to not take an additional dependency on external frameworks like MediatR. It's not needed as no advanced pipelining is used here.

## Tests
API integration tests for:
- [Opening shopping cart](./ECommerce.Api.Tests/ShoppingCarts/Opening/OpenShoppingCartTests.cs) as an example of creating a new entity,
- [Confirming shopping cart](./ECommerce.Api.Tests/ShoppingCarts/Confirming/ConfirmShoppingCartTests.cs) as an example of updating an existing entity,


## Prerequisites

1. Install git - https://git-scm.com/downloads.
2. Install .NET 10.0 - https://dotnet.microsoft.com/download/dotnet/10.0.
3. Install Visual Studio 2022, Rider or VSCode.
4. Install docker - https://docs.docker.com/engine/install/.
5. Open `ECommerce.slnx` solution.

## Running

1. Go to [docker](./docker) and run: `docker compose up`.
2. Wait until all containers are downloaded and running.
3. You should automatically get:
    - KurrentDB UI (for event store): http://localhost:2113/
    - Postgres DB running (for read models)
    - PG Admin - IDE for postgres. Available at: http://localhost:5050.
        - Login: `pgadmin4@pgadmin.org`, Password: `admin`
        - To connect to server Use host: `postgres`, user: `postgres`, password: `Password12!`
4. Open, build and run `ECommerce.slnx` solution.
    - Swagger should be available at: http://localhost:5000/index.html
