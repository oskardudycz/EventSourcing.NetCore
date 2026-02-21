# Exercise 17 - Multi-Stream Projections with Marten

Same as Exercise 15 but using Marten's `MultiStreamProjection<T, TId>` API.

## Goal

Learn how to use Marten's built-in multi-stream projection support to simplify correlation logic.

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

## Reference

- [Marten Multi-Stream Projections](https://martendb.io/events/projections/multi-stream-projections.html)
