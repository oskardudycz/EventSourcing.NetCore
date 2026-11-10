# Exercise 05 - Consistency

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

- Start from the idempotent saga of the previous exercise. Choose one path: [Aggregates](./Version1-Aggregates) or [Immutable Entities](./Version2-ImmutableEntities),
- Use a transactional outbox: store the state and the events to publish together in `database.Transaction(...)`, then publish the stored events and remove them from the outbox,
- A failed transaction leaves nothing behind, so it's safe to try it again. Publishing and removing a published event from the outbox can fail too, so retry them,
- Use `BusinessProcessTests`, `EntityDefinitionTests` and `IdempotencyTests` to verify if you implemented it correctly.

If an event gets published but not removed from the outbox, it will be published again. That's where the idempotency from the previous exercise pays off. Real systems also run a background process that publishes whatever was left in the outbox after a crash.
