# Exercise 04 - Idempotency

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

**Make the guest stay accounts and the group checkout handle the same message more than once.**

Message brokers usually guarantee at-least-once delivery, and clients retry requests when they don't get a response. Your handlers will see the same message twice, so handling it again must not change the outcome.

- Start from the saga solution of the previous exercise. Choose one path: [Aggregates](./Version1-Aggregates) or [Immutable Entities](./Version2-ImmutableEntities),
- Make the guest stay account ignore:
  - checking in the same guest stay twice,
  - recording the same charge twice (the same charge id),
  - recording the same payment twice (the same payment id),
  - checking out the guest twice for the same group checkout,
- Make the group checkout ignore:
  - initiating a group checkout that already exists,
  - recording the same guest checkout completion twice,
  - recording the same guest checkout failure twice,
- The whole process should give the same result when [the event bus](./Core/EventBus.cs) and [the command bus](./Core/CommandBus.cs) deliver every message twice,
- Use `IdempotencyTests` to guide you and verify if you implemented it correctly. `BusinessProcessTests` and `EntityDefinitionTests` should stay green.
