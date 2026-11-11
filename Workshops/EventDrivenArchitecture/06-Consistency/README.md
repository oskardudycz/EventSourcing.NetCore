# Exercise 06 - Consistency

Having the group checkout modelled in the previous exercises, and defined as:

**Group Checkout** is a quick way to check out all guests of the specific group. Clerk selects set of room stay based on the request from a person from the group and tries to check out all of them.

The main goal for this feature is to speed up existing single checkouts, as they're not efficient for bigger groups and other guests need to stand in the queue.

We still need to support regular checkouts, payments, charges etc. as we do right now.

**Rules:**

1. Guest can checkout only if the financial account balance is settled.
2. Guests cannot checkout if they already checked out.
3. Guests cannot checkout if they are not checked in.

**Notes:**

1. Room Stay means room + length of the stay + main guest
2. Settled means SUM(Charges) - SUM(Payments) === 0

## Goal

**Make sure the group checkout never loses a state change or an event, even when the database and the event bus fail now and then.**

Storing the state and then publishing its events is a dual write. If storing fails, the state change is lost. If publishing fails, the events are lost and the process gets stuck.

The [in-memory database](./Core/Database.cs) and [in-memory event bus](./Core/EventBus.cs) now simulate transient outages. A write or a publish fails with [TimeoutException](./Core/TimeoutException.cs) at a random call, and then again after 2 to 4 calls. The failures land in a different place on every run, so patching the call that failed last time won't help.

Use the [transactional outbox](https://event-driven.io/en/outbox_inbox_patterns_and_delivery_guarantees_explained/): store the messages to publish in the database, and publish them in the background. You can do it in two ways:

1. **Publish asynchronously.** Store the state and its events in the outbox in the same transaction. A background job publishes the events in the order they were stored and remembers the position of the last published one (a checkpoint). Published events stay in the outbox. Nothing gets lost and the order is kept.
2. **Publish right away.** Store the state and publish its events immediately. If publishing fails, store the events that weren't published in the outbox. A background job publishes them and removes each published one. It's best effort: events can arrive out of order, and a crash between storing the state and storing the events still loses them.

What you have:

- the saga with optimistic concurrency from the previous exercise as a starting point. Choose one path: [Aggregates](./Version1-Aggregates) or [Immutable Entities](./Version2-ImmutableEntities),
- `database.Transaction(...)`, which commits all writes made inside it together, or none of them. Storing with the expected version still works inside it, and the commit fails with `ConcurrencyException` on a conflicting change,
- `database.GetAll<T>(...)`, which returns documents in the order they were stored, each with the `Position` the database gave it,
- [background jobs](./Core/BackgroundJobs.cs) passed to the facades. Register a job there, and the tests will run it until it succeeds and stops publishing messages, as an application would do continuously.

Use `BusinessProcessTests`, `EntityDefinitionTests`, `IdempotencyTests` and `OptimisticConcurrencyTests` to verify if you implemented it correctly.

A failed write leaves nothing behind, so it's safe to try it again. If a background job publishes an event but fails to store the checkpoint or remove the event, it will publish it again. That's where the idempotency from exercise 04 pays off.
