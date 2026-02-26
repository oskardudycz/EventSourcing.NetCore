# Exercise 16 - Multi-Stream Projections with Out-of-Order Events

Fix the projection from Exercise 15 to handle out-of-order events.

## Goal

Learn how to build resilient projections that work even when events arrive in any order.

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
- 
## Context

Events can arrive out of order (e.g., from different RabbitMQ queues or Kafka topics). The projection from Exercise 15 was built assuming ordered events — run the test to see it fail.

For example, `FraudScoreCalculated` might fire before `PaymentRecorded`, meaning the Amount is 0 when you try to make the decision.

**Emit event when payment verification is completed**.

## Decision Logic

Only derive a final status when you have all three pieces of data (payment, merchant check, fraud check). Then apply the same rules as Exercise 15.

