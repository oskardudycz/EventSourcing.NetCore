# Exercise 18 - Multi-Stream Projections with Out-of-Order Events (Marten)

Fix the Marten projection from Exercise 17 to handle out-of-order events.

## Goal

Learn how to build resilient Marten projections that work even when events arrive in any order.

## Context

Same out-of-order context as Exercise 16: events can arrive in any order (e.g., from different RabbitMQ queues or Kafka topics). The projection from Exercise 17 assumes ordered events — run the test to see it fail.

**Emit event when payment verification is completed**.

## Reference

- [Marten Multi-Stream Projections](https://martendb.io/events/projections/multi-stream-projections.html)
