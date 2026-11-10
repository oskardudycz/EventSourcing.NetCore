# Exercise 03 - Business Processes

Having the group checkout modelled in the previous exercise, and defined as:

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

**Based on the business logic solution from the previous exercise, model the process and business logic of the Group Checkout.**

- Choose one path: [Aggregates](./Version1-Aggregates) or [Immutable Entities](./Version2-ImmutableEntities),
- Guest Stay Account logic is already there, together with [GuestStayFacade](./Version1-Aggregates/GuestStayFacade.cs). Its `InitiateGroupCheckout` publishes the event that starts the process,
- Define the Group Checkout entity and its application logic, using the [in-memory database](./Core/Database.cs) to store changes and the [in-memory event bus](./Core/EventBus.cs) to publish events as the outcome of business logic,
- Coordinate the process by subscribing to events on the event bus and handling commands on the [command bus](./Core/CommandBus.cs). Wire them in the `BusinessProcessTests` constructor (think about it as setup called once upon application startup),
- You can use any preferred style of implementing process coordination,
- Use `BusinessProcessTests` to guide you and verify if you implemented it correctly.

### Commands and events

Commands and events travel through separate buses here, which makes the difference in intent visible:

- a command asks for something to be done, e.g. `CheckOutGuest`. It has one intended recipient, which may accept or reject it, so the [command bus](./Core/CommandBus.cs) routes it to a single handler,
- an event records something that already happened, e.g. `GuestCheckedOut`. Any number of subscribers may react to it, or none, so the [event bus](./Core/EventBus.cs) delivers it to every subscriber.
