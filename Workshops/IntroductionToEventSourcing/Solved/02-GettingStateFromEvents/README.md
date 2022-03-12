# Exercise 02 - Getting the current entity state from events

Having a defined structure of events and an entity representing the shopping cart from the [previous exercise](../01-EventsDefinition), fill a `GetShoppingCart` function that will rebuild the current state from events.

If needed you can modify the events or entity structure.

There are two variations:
- using mutable entities: [Mutable/GettingStateFromEventsTests.cs](./Mutable/GettingStateFromEventsTests.cs),
- using fully immutable structures: [Immutable/Solution1/GettingStateFromEventsTests.cs](./Immutable/Solution1/GettingStateFromEventsTests.cs).

Select your preferred approach (or both) to solve this use case.

## Solution

Read also my article [How to get the current entity state from events?](https://event-driven.io/en/how_to_get_the_current_entity_state_in_event_sourcing/?utm_source=event_sourcing_net_workshop).

1. Mutable: [Mutable/GettingStateFromEventsTests.cs](./Mutable/GettingStateFromEventsTests.cs).
2. Immutable
  * Immutable entity using foreach with switch pattern matching [Immutable/Solution1/GettingStateFromEventsTests.cs](./Immutable/Solution1/GettingStateFromEventsTests.cs).
  * Fully immutable and functional with linq Aggregate method: [Immutable/GettingStateFromEventsTests.cs](./Immutable/Solution2/GettingStateFromEventsTests.cs).

