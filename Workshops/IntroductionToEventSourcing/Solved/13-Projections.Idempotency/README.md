# Exercise 13 - Projections Idempotency

With the [Database](./Tools/Database.cs) interface representing the sample database, implement the following projections:

1. Detailed view of the shopping cart:
    - total amount of products in the basket,
    - total number of products
    - list of products (e.g. if someone added the same product twice, then we should have one element with the sum).
2. View of the summary of the customer's purchases, where we have information about:
    - the number of all products in the confirmed shopping carts
    - the total amount of confirmed product items.

Add event handlers registrations in [ProjectionsTests](ProjectionsTests.cs) calling [EventBus.Register](./Tools/EventBus.cs) method.

Track and implement proper idempotency handling in projection event handlers.

If needed expand existing classes definition.

## Solutions

Read more in my articles:
- [Dealing with Eventual Consistency and Idempotency in MongoDB projections](https://event-driven.io/en/dealing_with_eventual_consistency_and_idempotency_in_mongodb_projections/?utm_source=event_sourcing_net_workshop)
- [A simple trick for idempotency handling in the Elastic Search read model](https://event-driven.io/en/simple_trick_for_idempotency_handling_in_elastic_search_readm_model/?utm_source=event_sourcing_net_workshop)
