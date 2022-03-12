# Exercise 04 - Appending events to Marten

Using a defined structure of events from the [previous exercise](../01-EventsDefinition), fill a `AppendEvents` function to store events in [EventStoreDB](https://developers.eventstore.com/clients/grpc/).

## Prerequisites
Run [docker-compose](../docker-compose.yml) script from the main workshop repository to start EventStoreDB instance.

```shell
docker-compose up
```

After that you can use EventStoreDB UI to see how streams and events look like. It's available at: http://localhost:2113/.
