using System.Collections;

namespace EventPipelines;

public interface IEventBus
{
    Task Publish(object @event, CancellationToken ct);
}

public interface IEventHandler
{
    Type CanHandle { get; }

    ValueTask<object?> Handle(object @event, CancellationToken ct);
}

public interface IEventHandler<in TEvent>: IEventHandler
{
    Type IEventHandler.CanHandle => typeof(TEvent);

    async ValueTask<object?> IEventHandler.Handle(object @event, CancellationToken ct)
    {
        await Handle((TEvent)@event, ct);
        return @event;
    }

    ValueTask Handle(TEvent @event, CancellationToken ct);
}

public class EventHandlerWrapper<TEvent>: IEventHandler<TEvent>
{
    private readonly Func<TEvent, CancellationToken, ValueTask> handler;

    public EventHandlerWrapper(Func<TEvent, CancellationToken, ValueTask> handler)
    {
        this.handler = handler;
    }

    public EventHandlerWrapper(Action<TEvent> handler)
    {
        this.handler = (@event, _) =>
        {
            handler(@event);
            return ValueTask.CompletedTask;
        };
    }

    public ValueTask Handle(TEvent @event, CancellationToken ct) =>
        handler(@event, ct);
}

public interface IEventTransformation<in TEvent, TTransformedEvent> : IEventHandler
{
    Type IEventHandler.CanHandle => typeof(TEvent);

    async ValueTask<object?> IEventHandler.Handle(object @event, CancellationToken ct)
    {
        return await Handle((TEvent)@event, ct);
    }

    ValueTask<TTransformedEvent> Handle(TEvent @event, CancellationToken ct);
}


public class EventTransformationWrapper<TEvent, TTransformedEvent>: IEventTransformation<TEvent, TTransformedEvent>
{
    private readonly Func<TEvent, CancellationToken, ValueTask<TTransformedEvent>> handler;

    public EventTransformationWrapper(Func<TEvent, CancellationToken, ValueTask<TTransformedEvent>> handler)
    {
        this.handler = handler;
    }

    public EventTransformationWrapper(Func<TEvent, TTransformedEvent> handler)
    {
        this.handler = (@event, _) =>
        {
            var result = handler(@event);
            return ValueTask.FromResult(result);
        };
    }

    public ValueTask<TTransformedEvent> Handle(TEvent @event, CancellationToken ct) =>
        handler(@event, ct);
}

public class EventTransformationWrapper<TEvent>: EventTransformationWrapper<TEvent, object>
{
    public EventTransformationWrapper(Func<TEvent, CancellationToken, ValueTask<object>> handler) : base(handler)
    {
    }

    public EventTransformationWrapper(Func<TEvent, object> handler) : base(handler)
    {
    }
}

public interface IEventFilter<in TEvent> : IEventHandler
{
    Type IEventHandler.CanHandle => typeof(TEvent);

    async ValueTask<object?> IEventHandler.Handle(object @event, CancellationToken ct)
    {
        var matches = await Handle((TEvent)@event, ct);

        return matches ? @event : null;
    }

    ValueTask<bool> Handle(TEvent @event, CancellationToken ct);
}

public class EventFilterWrapper<TEvent>: IEventFilter<TEvent>
{
    private readonly Func<TEvent, CancellationToken, ValueTask<bool>> handler;

    public EventFilterWrapper(Func<TEvent, CancellationToken, ValueTask<bool>> handler)
    {
        this.handler = handler;
    }

    public EventFilterWrapper(Func<TEvent, bool> handler)
    {
        this.handler = (@event, _) =>
        {
            var result = handler(@event);
            return ValueTask.FromResult(result);
        };
    }

    public ValueTask<bool> Handle(TEvent @event, CancellationToken ct) =>
        handler(@event, ct);
}

public class EventBus: IEventBus
{
    private readonly IDictionary<Type, List<IEventHandler>> handlers;

    public EventBus(
        IEnumerable<IEventHandler> handlers
    )
    {
        this.handlers = handlers
            .GroupBy(h => h.CanHandle)
            .ToDictionary(h => h.Key, h => h.ToList());
    }

    public async Task Publish(object @event, CancellationToken ct)
    {
        var eventType = @event.GetType();

        if (!handlers.TryGetValue(eventType, out var eventHandlers))
            return;

        foreach (var handler in eventHandlers)
        {
            var result = await handler.Handle(@event, ct);

            if(result == null)
                break;

            if (result == @event)
                continue;

            if (result is IEnumerable enumerable)
            {
                foreach (var newEvent in enumerable)
                {
                    await Publish(newEvent, ct);
                }
                return;
            }
            await Publish(result, ct);
        }
    }
}
