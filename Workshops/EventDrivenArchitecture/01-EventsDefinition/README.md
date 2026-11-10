# Exercise 01 - Events Definition

Having the group checkout defined as:

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

**Model the events that occur during this process.** Define those events in code:

- Guest Stay Account events in [GuestStayAccountEvents](./GuestStayAccountEvents.cs),
- Group Checkout events in [GroupCheckoutEvents](./GroupCheckoutEvents.cs).

Create sample events that represent the process. You can do that in [EventsDefinitionTests](./EventsDefinitionTests.cs) test file.
