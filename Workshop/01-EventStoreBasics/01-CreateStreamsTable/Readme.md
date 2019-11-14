# Exercise 01 - Create Streams Table

## Overview

Stream represents ordered set of events. Simply speaking stream is an log of the events that happened for the
specific aggregate/entity.

As truth is in the log, so in the events - stream can be also interpreted as the grouping, where key is stream id.

`Id` - needs to be unique, so normally is represented by Guid type.

It's also common to add two other columns:
* `Type` - information about the stream type. It' mostly used to make debugging easier or some optimizations,
* `Version` - auto-incremented value used mostly for optimistic concurrency check.

See more in Greg Young's "Building an Event Storage" [article](https://cqrs.wordpress.com/documents/building-event-storage/) 

## Description
In this exercise you should write the code that will create proper Streams table. To do that you need to open [EventStore class](./EventStore.cs) that has already some boilerplate defined and placeholder in the [CreateStreamTable method](./EventStore.cs#L23).

## Dapper
It's suggested to use [Dapper](https://github.com/StackExchange/Dapper/) to perform the database calls. `Dapper` is a simple object mapper for .Net provided by and used in [StackOverlfow](https://stackoverflow.com/). It simplifies the mapping between the raw SQL commands/queries and the objects.

To execute command without the result you can use [Execute](https://github.com/StackExchange/Dapper/#execute-a-command-that-returns-no-results) method. It will automatically open connection, so you don't need to do it manually.

## Postgres
As the database we'll use `Postgres`. It's an open source, mature and well designed and performant database. 
It also has great support for `JSON` (probably the best from the popular relational databases) - see more [here](http://www.postgresqltutorial.com/postgresql-json/) or [there](https://blog.codeship.com/unleash-the-power-of-storing-json-in-postgres/).

For the syntax of the table creation you can refer to the [official documentation](http://www.postgresqltutorial.com/postgresql-create-table/).

You can find mappings of the Postgres column types to the .NET types in [Npgsql documentation](https://www.npgsql.org/doc/types/basic.html) 

_**Note**: If you prefer to use different database feel free to do that, all of the exercises and samples of creating own Event Store should be easily transferable to other database engines._

