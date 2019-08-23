# Exercise 01 - Create Streams Table

Stream represents ordered set of events. Simply speaking stream is an log of the events that happened for the
specific aggregate/entity.

As truth is in the log, so in the events - stream can be also interpreted as the grouping, where key is stream id.

`Id` - needs to be unique, so normally is represented by Guid type.

It's also common to add two other columns:
* `Type` - information about the stream type. It' mostly used to make debugging easier or some optimizations.
* `Version` - auto-incremented value used mostly for optimistic concurrency check


Greg Young's "Building an Event Storage" article https://cqrs.wordpress.com/documents/building-event-storage/
Dapper: https://github.com/StackExchange/Dapper/
