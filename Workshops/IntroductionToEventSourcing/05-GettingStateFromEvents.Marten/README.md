# Exercise 05 - Getting the current entity state from events using Marten

Having a defined structure of events and an entity representing the shopping cart from the [previous exercise](../01-EventsDefinition), fill a `GetShoppingCart` function that will rebuild the current state from events.

If needed you can modify the events or entity structure.

There are two variations:
- using mutable entities: [Mutable/GettingStateFromEventsTests.cs](./Mutable/GettingStateFromEventsTests.cs),
- using fully immutable structures: [Immutable/Solution1/GettingStateFromEventsTests.cs](./Immutable/GettingStateFromEventsTests.cs).

Select your preferred approach (or both) to solve this use case. If needed you can modify entities or events.

## Prerequisites
Run [docker-compose](../docker-compose.yml) script from the main workshop repository to start Postgres instance.

```shell
docker-compose up
```

After that you can use PG admin (IDE for Postgres) to see how tables and data look like. It's available at: http://localhost:5050.
- Login: `admin@pgadmin.org`, Password: `admin`
- To connect to server click right mouse on Servers, then Register Server and use host: `postgres`, user: `postgres`, password: `Password12!`
