# Exercise 16 - Multi-Stream Projections with Out-of-Order Events

In exercise 15 you built multi-stream projections assuming events arrive in a predictable order. The real world isn't that kind. Events arrive **in any order**, especially when they come from different services.

## Scenario: Payment Verification with Race Conditions

The same payment verification domain, but now events arrive scrambled:

1. **FraudScoreCalculated** might arrive before **PaymentRecorded**
2. **MerchantLimitsChecked** could be first or last
3. No **PaymentVerificationCompleted** event — your projection derives the decision when enough data arrives

This teaches you to build **resilient read models** using the phantom record pattern.

## Key Differences from Exercise 15

1. **No final decision event** — the projection determines approval/rejection based on available data
2. **Handle partial state** — the read model exists even with incomplete information
3. **Derive status** — when you have all required data, calculate the final status

## What to implement

With the [Database](./Tools/Database.cs) interface representing the sample database, implement a resilient `PaymentVerification` projection:

1. Define additional `PaymentVerification` properties to store data from each event — but design it to handle missing data
2. Create a `PaymentVerificationProjection` class with typed `Handle` methods for each event
3. Each handler should work even if other events haven't arrived yet
4. When enough data exists, derive the final `Status` (Approved/Rejected/Pending)
5. Register handlers in the test using `eventStore.Register`

The test will append events **in scrambled order** and verify your projection handles partial state correctly.

## Reference

Read more about handling out-of-order events and phantom records:
- [Dealing with Race Conditions in Event-Driven Architecture](https://www.architecture-weekly.com/p/dealing-with-race-conditions-in-event)
