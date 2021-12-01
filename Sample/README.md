
# Samples

Samples are using CQRS architecture. They're sliced based on the business modules and operations. Read more about the assumptions in ["How to slice the codebase effectively?"](https://event-driven.io/en/how_to_slice_the_codebase_effectively/?utm_source=event_sourcing_net).

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

## [Event Versioning](./EventVersioning)
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
