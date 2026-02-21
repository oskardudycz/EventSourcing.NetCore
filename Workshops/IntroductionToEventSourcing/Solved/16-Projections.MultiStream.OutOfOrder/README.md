# Exercise 16 - Multi-Stream Projections with Out-of-Order Events

Fix the projection from Exercise 15 to handle out-of-order events.

## Goal

Learn how to build resilient projections that work even when events arrive in any order.

## Context

Events can arrive out of order (e.g., from different RabbitMQ queues or Kafka topics). The projection from Exercise 15 was built assuming ordered events — run the test to see it fail.

For example, `FraudScoreCalculated` might fire before `PaymentRecorded`, meaning the Amount is 0 when you try to make the decision.

## Steps to Fix

1. **Make fields nullable**: Change `OrderId` and `Amount` to nullable types (`Guid?`, `decimal?`)
2. **Add a Recalculate method**: Create a method that derives the status based on available data
3. **Call Recalculate from every handler**: Each handler updates its data, then calls `Recalculate()`
4. **Track data quality**: Add a `DataQuality` enum (Partial, Sufficient, Complete) to track completeness
5. **Inject EventStore**: Inject `EventStore` into the projection to publish `PaymentVerificationCompleted` when a decision is made

## Decision Logic

Only derive a final status when you have all three pieces of data (payment, merchant check, fraud check). Then apply the same rules as Exercise 15.

## Reference

- [Dealing with Race Conditions in Event-Driven Architecture](https://www.architecture-weekly.com/p/dealing-with-race-conditions-in-event)
