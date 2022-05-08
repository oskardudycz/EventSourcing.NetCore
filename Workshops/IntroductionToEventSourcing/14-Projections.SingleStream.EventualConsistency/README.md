# Exercise 14 - Projections Eventual Consistency

With the [Database](./Tools/Database.cs) interface representing the sample database, implement the following projections

1. Detailed view of the shopping cart:
    - total amount of products in the basket,
    - total number of products
    - list of products (e.g. if someone added the same product twice, then we should have one element with the sum).
2. View with short information about pending shopping carts. It's intended to be used as list view for administration:
    - total amount of products in the basket,
    - total number of products
    - confirmed and canceled shopping carts should not be visible.

Add event handlers registrations in [ProjectionsTests](ProjectionsTests.cs) calling [EventBus.Register](./Tools/EventBus.cs) method.

Track and implement proper idempotency and eventual consistency handling in projection event handlers.

If needed expand existing classes definition.
