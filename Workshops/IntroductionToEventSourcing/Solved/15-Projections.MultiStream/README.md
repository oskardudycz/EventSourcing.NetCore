# Exercise 15 - Multi-Stream Projections

Build a multi-stream projection that correlates events from different streams by `PaymentId`.

## Goal

Learn how to build projections that combine events from multiple event streams into a single read model.

## Scenario

Events arrive from three different streams (payment, merchant, and fraud check), but they all reference the same `PaymentId`. Your projection must:

1. Collect data from all three event types
2. Store them in a single `PaymentVerification` read model
3. Derive the payment verification status when all data is present

## Steps

1. Create a `PaymentVerificationProjection` class with `Handle` methods for each event type
2. Register your handlers using `eventStore.Register`
3. Implement decision logic in the `FraudScoreCalculated` handler (always last for completed payments):
   - Reject if merchant failed
   - Reject if fraud score > 0.75
   - Reject if amount > 10000 AND fraud score > 0.5
   - Otherwise approve

## Key Concept

Events arrive on **different stream IDs**, but all reference the same `PaymentId`. Use `PaymentId` (not the stream ID) as the read model key.

## Reference

- [Handling Events Coming in an Unknown Order](https://www.architecture-weekly.com/p/handling-events-coming-in-an-unknown)
