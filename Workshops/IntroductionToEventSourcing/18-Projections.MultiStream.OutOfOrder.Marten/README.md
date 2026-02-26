# Exercise 18 - Multi-Stream Projections with Out-of-Order Events (Marten)

Fix the Marten projection from Exercise 17 to handle out-of-order events.

## Goal

Learn how to build resilient Marten projections that work even when events arrive in any order.

## Scenario

Events arrive from three different streams (payment, merchant, and fraud check), but they all reference the same `PaymentId`. Your projection must:

1. Collect data from all three event types
2. Store them in a single `PaymentVerification` read model
3. Derive the payment verification status when all data is present

Decision logic:
- Reject if merchant failed
- Reject if fraud score > 0.75
- Reject if amount > 10000 AND fraud score > 0.5
- Otherwise approve

## Context

Same out-of-order context as Exercise 16: events can arrive in any order (e.g., from different RabbitMQ queues or Kafka topics). The projection from Exercise 17 assumes ordered events — run the test to see it fail.

**Emit event when payment verification is completed**. Use [`RaiseSideEffects`method from Marten projections](https://martendb.io/events/projections/side-effects.html#side-effects)

## Reference

- [Marten Multi-Stream Projections](https://martendb.io/events/projections/multi-stream-projections.html)
- [Marten Async Daemon - Backround worker processing async projections](https://martendb.io/events/projections/async-daemon.html#async-projections-daemon)
- [`RaiseSideEffects` method from Marten projections](https://martendb.io/events/projections/side-effects.html#side-effects)

