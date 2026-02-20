# Exercise 15 - Multi-Stream Projections

In exercises 12-14 you built projections from a single event stream. Now you'll combine events from **multiple streams** into a single read model.

## Scenario: Payment Verification

A payment verification requires data from three independent checks, each producing events on its own stream:

1. **Payment recorded** — from the payment service (amount, order reference)
2. **Merchant limits checked** — from the merchant service (within daily limits?)
3. **Fraud score calculated** — from the fraud detection service (risk score, acceptable?)
4. **Verification completed** — final decision event (approved or rejected)

All events share a `PaymentId` that ties them to the same payment verification read model.

## What to implement

With the [Database](./Tools/Database.cs) interface representing the sample database, implement a `PaymentVerification` read model and projection:

1. Define the `PaymentVerification` read model properties — the test assertions tell you what shape it needs.
2. Create a `PaymentVerificationProjection` class with typed `Handle` methods for each event.
3. Register handlers in the test using `eventStore.Register`.

The key difference from single-stream projections: each event arrives on a **different stream ID**, but they all reference the same `PaymentId`. Your projection must use `PaymentId` (not the stream ID) as the read model key.

## Reference

Read more about multi-stream projections and handling events from multiple sources:
- [Handling Events Coming in an Unknown Order](https://www.architecture-weekly.com/p/handling-events-coming-in-an-unknown)
