# Exercise 06 - Getting the current entity state from events using Marten

Having a defined structure of events and an entity representing the shopping cart from the [first exercise](../01-EventsDefinition), fill a `GetShoppingCart` function that will rebuild the current state from events.

If needed you can modify the events or entity structure.

There are two variations:
- using mutable entities: [Mutable/GettingStateFromEventsTests.cs](./Mutable/GettingStateFromEventsTests.cs),
- using fully immutable structures: [Immutable/Solution1/GettingStateFromEventsTests.cs](./Immutable/GettingStateFromEventsTests.cs).

Select your preferred approach (or both) to solve this use case. If needed you can modify entities or events.

## Prerequisites
Run [docker compose](../../docker-compose.yml) script from the main workshop repository to start EventStoreDB instance.

```shell
docker compose up
```

After that you can use EventStoreDB UI to see how streams and events look like. It's available at: http://localhost:2113/.

## Solution

Read also my articles:
- [How to get the current entity state from events?](https://event-driven.io/en/how_to_get_the_current_entity_state_in_event_sourcing/?utm_source=event_sourcing_net_workshop).
- [Should you throw an exception when rebuilding the state from events?](https://event-driven.io/en/should_you_throw_exception_when_rebuilding_state_from_events/?utm_source=event_sourcing_net_workshop)
- and [EventStoreDB documentation on reading events](https://developers.eventstore.com/clients/grpc/reading-events.html#reading-from-a-stream)

1. Mutable: [Mutable/GettingStateFromEventsTests.cs](./Mutable/GettingStateFromEventsTests.cs).
2. Immutable: [Immutable/GettingStateFromEventsTests.cs](./Immutable/GettingStateFromEventsTests.cs).
