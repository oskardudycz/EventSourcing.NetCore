# Exercise 17 - Multi-Stream Projections with Marten

In exercises 15-16 you built multi-stream projections manually. Now you'll use Marten's `MultiStreamProjection` to handle the complexity for you.

## Scenario: Payment Verification with Marten

The same payment verification domain from exercise 15, but using Marten's built-in multi-stream projection support.

## What to implement

Implement a `PaymentVerificationProjection` using Marten's `MultiStreamProjection<PaymentVerification, Guid>`:

1. Inherit from `MultiStreamProjection<PaymentVerification, Guid>`
2. In the constructor, use `Identity<TEvent>` to specify how each event type maps to the `PaymentId`
3. Define `Apply` methods for each event type to update the read model
4. Register the projection with Marten's projection system in the test

## Marten Multi-Stream Projection Pattern

```csharp
public class YourProjection: MultiStreamProjection<YourReadModel, Guid>
{
    public YourProjection()
    {
        // Tell Marten which property on each event contains the document ID
        Identity<Event1>(e => e.DocumentId);
        Identity<Event2>(e => e.DocumentId);
    }

    // Define how each event updates the read model
    public void Apply(YourReadModel model, Event1 @event)
    {
        // Update model based on event
    }
}
```

##Reference

Read more about Marten's multi-stream projections:
- [Marten Multi-Stream Projections](https://martendb.io/events/projections/multi-stream-projections.html)
