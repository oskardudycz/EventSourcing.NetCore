# Exercise 04 - Appending events to KurrentDB

Using a defined structure of events from the [first exercise](../01-EventsDefinition), fill a `AppendEvents` function to store events in [KurrentDB](https://docs.kurrent.io/clients/dotnet/v1.4/).

## Prerequisites
Run [docker compose](../docker-compose.yml) script from the main workshop repository to start KurrentDB instance.

```shell
docker compose --profile all up
```

After that you can use KurrentDB UI to see how streams and events look like. It's available at: http://localhost:2113/.
