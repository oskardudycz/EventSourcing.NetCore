# Exercise 14 - Projections Eventual Consistency

With the [Database](./Tools/Database.cs) interface representing the sample database, implement the following projections:

1. Detailed view of the shopping cart:
    - total amount of products in the basket,
    - total number of products
    - list of products (e.g. if someone added the same product twice, then we should have one element with the sum).
2. View with short information about pending shopping carts. It's intended to be used as list view for administration:
    - total amount of products in the basket,
    - total number of products
    - confirmed and canceled shopping carts should not be visible.

Add event handlers registrations in [ProjectionsTests](ProjectionsTests.cs) calling [EventStore.Register](./Tools/EventStore.cs) method.

Track and implement proper idempotency and eventual consistency handling in projection event handlers.

## Solutions

Read more in my articles:
- [Dealing with Eventual Consistency and Idempotency in MongoDB projections](https://event-driven.io/en/dealing_with_eventual_consistency_and_idempotency_in_mongodb_projections/?utm_source=event_sourcing_net_workshop)
- [A simple trick for idempotency handling in the Elastic Search read model](https://event-driven.io/en/simple_trick_for_idempotency_handling_in_elastic_search_readm_model/?utm_source=event_sourcing_net_workshop)
