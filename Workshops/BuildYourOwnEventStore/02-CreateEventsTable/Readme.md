# Exercise 02- Create Events Table

Events table is the main table for Event Sourcing storage. It contains the information about events
that occurred in the system. Each event is stored in separate row.

They're stored as key/value pair (id + event data) plus additional data like stream id, version, type, creation timestamp.

So the full list of the Events Table columns is:
* `Id` - unique event identifier
* `Data` - Event data serialized as JSON
* `StreamId` - id of the stream that event occured
* `Type` - information about the event type. It's used to understand what's that event all about, e.g. `OrderRegistered`. Used also for getting info about the type to deserialise.
* `Version` - version of the stream at which event occured used for keeping sequence of the event and for optimistic concurrency check
* `Created` - Timestamp at which event was created. Used to get the state of the stream at exact time.
