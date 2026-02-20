# Exercise 18 - Multi-Stream Projections with Out-of-Order Events (Marten)

Combining exercises 16 and 17: handle out-of-order events from multiple streams using Marten's `MultiStreamProjection`.

## Scenario: Payment Verification with Out-of-Order Events (Marten)

Events arrive in any order, and you need a resilient projection that:
- Handles partial state
- Derives decisions from available data
- Uses Marten's multi-stream projection capabilities

## Key Differences from Exercise 17

1. **No final decision event** — the projection determines approval/rejection based on available data
2. **Handle partial state** — the read model exists even with incomplete information
3. **Derive status** — when you have all required data, calculate the final status using Marten

## What to implement

Implement a resilient `PaymentVerificationProjection` using Marten's `MultiStreamProjection<PaymentVerification, Guid>`:

1. Inherit from `MultiStreamProjection<PaymentVerification, Guid>`
2. Use `Identity<TEvent>` to map each event to the PaymentId
3. Define `Apply` methods that work even if other events haven't arrived yet
4. Implement logic to derive the final `Status` when enough data exists
5. Register the projection with Marten's projection system

The test will append events **in scrambled order** and verify your projection handles partial state correctly.

## Reference

- [Dealing with Race Conditions in Event-Driven Architecture](https://www.architecture-weekly.com/p/dealing-with-race-conditions-in-event)
- [Marten Multi-Stream Projections](https://martendb.io/events/projections/multi-stream-projections.html)
