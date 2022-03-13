# Exercise 03 - Appending events to Marten

Using a defined structure of events from the [previous exercise](../01-EventsDefinition), fill a `AppendEvents` function to store events in [Marten](https://martendb.io).

## Prerequisites
Run [docker-compose](../../docker-compose.yml) script from the main workshop repository to start Postgres instance.

```shell
docker-compose up
```

After that you can use PG admin (IDE for Postgres) to see how tables and data look like. It's available at: http://localhost:5050.
- Login: `admin@pgadmin.org`, Password: `admin`
- To connect to server click right mouse on Servers, then Register Server and use host: `postgres`, user: `postgres`, password: `Password12!`

## Solution

Use [Marten append events API](https://martendb.io/events/appending.html#appending-events-1) and save changes using `DocumentSession.SaveChangesAsync` method.
