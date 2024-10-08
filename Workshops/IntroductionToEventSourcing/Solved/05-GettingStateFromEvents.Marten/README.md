# Exercise 05 - Getting the current entity state from events using Marten

Having a defined structure of events and an entity representing the shopping cart from the [first exercise](../01-EventsDefinition), fill a `GetShoppingCart` function that will rebuild the current state from events.

If needed you can modify the events or entity structure.

There are two variations:
- using mutable entities: [Mutable/GettingStateFromEventsTests.cs](./Mutable/GettingStateFromEventsTests.cs),
- using fully immutable structures: [Immutable/Solution1/GettingStateFromEventsTests.cs](./Immutable/GettingStateFromEventsTests.cs).

Select your preferred approach (or both) to solve this use case. If needed you can modify entities or events.

## Prerequisites
Run [docker compose](../../docker-compose.yml) script from the main workshop repository to start Postgres instance.

```shell
docker compose up
```

After that you can use PG Admin (IDE for Postgres) to see how tables and data look like. It's available at: http://localhost:5050.
- Login: `admin@pgadmin.org`, Password: `admin`
- To connect to server click right mouse on Servers, then Register Server and use host: `postgres`, user: `postgres`, password: `Password12!`

## Solution

Read also my articles:
- [How to get the current entity state from events?](https://event-driven.io/en/how_to_get_the_current_entity_state_in_event_sourcing/?utm_source=event_sourcing_net_workshop).
- [Should you throw an exception when rebuilding the state from events?](https://event-driven.io/en/should_you_throw_exception_when_rebuilding_state_from_events/?utm_source=event_sourcing_net_workshop)
- and [Marten documentation on live aggregation](https://martendb.io/events/projections/live-aggregates.html)

1. Mutable: [Mutable/GettingStateFromEventsTests.cs](./Mutable/GettingStateFromEventsTests.cs).
2. Immutable:
 - with default Marten convention having Apply method per event type: [Immutable/DefaultConvention/GettingStateFromEventsTests.cs](./Immutable/DefaultConvention/GettingStateFromEventsTests.cs).
 - with a single Apply method using Shopping Cart Event union type: [Immutable/SingleApply/GettingStateFromEventsTests.cs](./Immutable/SingleApply/GettingStateFromEventsTests.cs).
