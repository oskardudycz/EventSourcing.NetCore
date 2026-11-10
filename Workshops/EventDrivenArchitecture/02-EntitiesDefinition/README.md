# Exercise 02 - Entities Definition

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

**Model the entity and business logic of the Guest Stay Account.**

- Define the entity in a new file, based on [Guest Stay Account events](./GuestStayAccountEvents.cs),
- Fill application logic in [GuestStayFacade](./GuestStayFacade.cs), using the [in-memory database](./Core/Database.cs) and [in-memory event bus](./Core/EventBus.cs) to store changes and publish events as the outcome of business logic,
- You can use any preferred style of implementing business logic,
- Use [EntityDefinitionTests](./EntityDefinitionTests.cs) to guide you and verify if you implemented it correctly.
