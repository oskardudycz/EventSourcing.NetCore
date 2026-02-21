# Exercise 17 - Multi-Stream Projections with Marten

Same as Exercise 15 but using Marten's `MultiStreamProjection<T, TId>` API.

## Goal

Learn how to use Marten's built-in multi-stream projection support to simplify correlation logic.

## Steps

1. Create a projection class that inherits from `MultiStreamProjection<PaymentVerification, Guid>`
2. Register Identity mappings in the constructor using `Identity<TEvent>(e => e.PaymentId)`
3. Implement `Apply` methods for each event type
4. Put decision logic in the `FraudScoreCalculated` Apply method (same rules as Exercise 15)
5. Register the projection using `options.Projections.Add<PaymentVerificationProjection>(ProjectionLifecycle.Inline)`

## Key Differences from Exercise 15

Instead of manually handling event routing and database operations, Marten:
- Automatically routes events to the correct document based on your Identity mappings
- Manages document loading, updating, and saving
- Provides a cleaner, declarative API

## Reference

- [Marten Multi-Stream Projections](https://martendb.io/events/projections/multi-stream-projections.html)
