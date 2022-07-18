
[![Twitter Follow](https://img.shields.io/twitter/follow/oskar_at_net?style=social)](https://twitter.com/oskar_at_net) [![Github Sponsors](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&link=https://github.com/sponsors/oskardudycz/)](https://github.com/sponsors/oskardudycz/) [![blog](https://img.shields.io/badge/blog-event--driven.io-brightgreen)](https://event-driven.io/?utm_source=event_sourcing_jvm) [![blog](https://img.shields.io/badge/%F0%9F%9A%80-Architecture%20Weekly-important)](https://www.architecture-weekly.com/?utm_source=event_sourcing_net) 

# Samples

Samples are using CQRS architecture. They're sliced based on the business modules and operations. Read more about the assumptions in ["How to slice the codebase effectively?"](https://event-driven.io/en/how_to_slice_the_codebase_effectively/?utm_source=event_sourcing_net).

## [Pragmatic Event Sourcing With Marten](./Helpdesk)
- Simplest CQRS and Event Sourcing flow using Minimal API,
- Cutting the number of layers and boilerplate complex code to bare minimum,
- Using all Marten helpers like `WriteToAggregate`, `AggregateStream` to simplify the processing,
- Examples of all the typical Marten's projections,
- Example of how and where to use C# Records, Nullable Reference Types, etc,
- No Aggregates. Commands are handled in the domain service as pure functions.

## [ECommerce with Marten](./ECommerce)
- typical Event Sourcing and CQRS flow,
- DDD using Aggregates,
- microservices example,
- stores events to Marten,
- distributed processes coordinated by Saga ([Order Saga](./ECommerce/Orders/Orders/Orders/OrderSaga.cs)),
- Kafka as a messaging platform to integrate microservices,
- example of the case when some services are event-sourced ([Carts](./ECommerce/Carts), [Orders](./ECommerce/Orders), [Payments](./ECommerce/Payments)) and some are not ([Shipments](./ECommerce/Shipments) using EntityFramework as ORM)

## [Simple EventSourcing with EventStoreDB](./EventStoreDB/Simple)
- typical Event Sourcing and CQRS flow,
- functional composition, no aggregates, just data and functions,
- stores events to  EventStoreDB,
- Builds read models using [Subscription to `$all`](https://developers.eventstore.com/clients/grpc/subscribing-to-streams/#subscribing-to-all),
- Read models are stored as Postgres tables using EntityFramework.

## [ECommerce with EventStoreDB](./EventStoreDB/ECommerce) 
- typical Event Sourcing and CQRS flow,
- DDD using Aggregates,
- stores events to  EventStoreDB,
- Builds read models using [Subscription to `$all`](https://developers.eventstore.com/clients/grpc/subscribing-to-streams/#subscribing-to-all).
- Read models are stored as Marten documents.

## [Warehouse](./Warehouse)
- simplest CQRS flow using .NET 5 Endpoints,
- example of how and where to use C# Records, Nullable Reference Types, etc,
- No Event Sourcing! Using Entity Framework to show that CQRS is not bounded to Event Sourcing or any type of storage,
- No Aggregates! CQRS do not need DDD. Business logic can be handled in handlers.

## [Warehouse Minimal API](./Warehouse.MinimalAPI/)
Variation of the previous example, but:
- using Minimal API,
- example how to inject handlers in MediatR like style to decouple API from handlers.
- 📝 Read more [CQRS is simpler than you think with .NET 6 and C# 10](https://event-driven.io/en/cqrs_is_simpler_than_you_think_with_net6/?utm_source=event_sourcing_net) 

## [Event Versioning](./EventsVersioning)
Shows how to handle basic event schema versioning scenarios using event and stream transformations (e.g. upcasting):
- [Simple mapping](./EventsVersioning/#simple-mapping)
  - [New not required property](./EventsVersioning/#new-not-required-property)
  - [New required property](./EventsVersioning/#new-required-property)
  - [Renamed property](./EventsVersioning/#renamed-property)
- [Upcasting](./EventsVersioning/#upcasting)
  - [Changed Structure](./EventsVersioning/#changed-structure)
  - [New required property](./EventsVersioning/#new-required-property-1)
- [Downcasters](./EventsVersioning/#downcasters)
- [Events Transformations](./EventsVersioning/#events-transformations)
- [Stream Transformation](./EventsVersioning/#stream-transformation)
- [Summary](./EventsVersioning/#summary)

## [Event Pipelines](./EventPipelines)
Shows how to compose event handlers in the processing pipelines to:
- filter events,
- transform them,
- NOT requiring marker interfaces for events,
- NOT requiring marker interfaces for handlers,
- enables composition through regular functions,
- allows using interfaces and classes if you want to,
- can be used with Dependency Injection, but also without through builder,
- integrates with MediatR if you want to.

## [Meetings Management with Marten](./MeetingsManagement/)
- typical Event Sourcing and CQRS flow,
- DDD using Aggregates,
- microservices example,
- stores events to Marten,
- Kafka as a messaging platform to integrate microservices,
- read models handled in separate microservice and stored to other database (ElasticSearch)

## [Cinema Tickets Reservations with Marten](./Tickets/)
- typical Event Sourcing and CQRS flow,
- DDD using Aggregates,
- stores events to Marten.

## [SmartHome IoT with Marten](./AsyncProjections/)
- typical Event Sourcing and CQRS flow,
- DDD using Aggregates,
- stores events to Marten,
- asynchronous projections rebuild using AsynDaemon feature.
