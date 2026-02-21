# Exercise 18 - Multi-Stream Projections with Out-of-Order Events (Marten)

Fix the Marten projection from Exercise 17 to handle out-of-order events.

## Goal

Learn how to build resilient Marten projections that work even when events arrive in any order.

## Context

Same out-of-order context as Exercise 16: events can arrive in any order (e.g., from different RabbitMQ queues or Kafka topics). The projection from Exercise 17 assumes ordered events — run the test to see it fail.

## Steps to Fix

1. **Make fields nullable**: Change `OrderId` and `Amount` to nullable types (`Guid?`, `decimal?`)
2. **Add Recalculate method on read model**: Create a `Recalculate()` method on the `PaymentVerification` class that derives status based on available data
3. **Call Recalculate from every Apply**: Each Apply method updates its data, then calls `item.Recalculate()`
4. **Track data quality**: Add a `DataQuality` enum and field to track completeness
5. **Use RaiseSideEffects**: Override `RaiseSideEffects` to publish `PaymentVerificationCompleted` using `slice.AppendEvent()` when a decision is made

## RaiseSideEffects Pattern

```csharp
public override ValueTask RaiseSideEffects(
    IDocumentOperations operations,
    IEventSlice<PaymentVerification> slice)
{
    var item = slice.Aggregate;
    if (item is { Status: not PaymentStatus.Pending })
    {
        slice.AppendEvent(new PaymentVerificationCompleted(
            item.Id, item.Status == PaymentStatus.Approved));
    }
    return ValueTask.CompletedTask;
}
```

## Reference

- [Dealing with Race Conditions in Event-Driven Architecture](https://www.architecture-weekly.com/p/dealing-with-race-conditions-in-event)
- [Marten Multi-Stream Projections](https://martendb.io/events/projections/multi-stream-projections.html)
