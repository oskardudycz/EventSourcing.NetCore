namespace EventPipelines;

public class EventHandlersBuilder
{
    private readonly List<IEventHandler> eventHandlers = new();

    private EventHandlersBuilder()
    {

    }

    public static EventHandlersBuilder Setup() => new();

    public EventHandlersBuilder Handle<T>(T eventHandler)
        where T : class, IEventHandler
    {
        eventHandlers.Add(eventHandler);
        return this;
    }

    public EventHandlersBuilder Handle<TEvent>(
        Action<TEvent> handler) =>
        Handle(new EventHandlerWrapper<TEvent>(handler));

    public EventHandlersBuilder Handle<TEvent>(
        Func<TEvent, CancellationToken, ValueTask> handler) =>
        Handle(new EventHandlerWrapper<TEvent>(handler));

    public EventHandlersBuilder Handle<TEvent>(
        IEventHandler<TEvent> handler) =>
        Handle(handler as IEventHandler);

    public EventHandlersBuilder Transform<TEvent, TTransformedEvent>(
        Func<TEvent, TTransformedEvent> handler) =>
        Handle(new EventTransformationWrapper<TEvent, TTransformedEvent>(handler));


    public EventHandlersBuilder Transform<TEvent>(
        Func<TEvent, object> handler) =>
        Handle(new EventTransformationWrapper<TEvent>(handler));

    public EventHandlersBuilder Transform<TEvent, TTransformedEvent>(
        Func<TEvent, CancellationToken, ValueTask<TTransformedEvent>> handler) =>
        Handle(new EventTransformationWrapper<TEvent, TTransformedEvent>(handler));


    public EventHandlersBuilder Transform<TEvent>(
        Func<TEvent, CancellationToken, ValueTask<object>> handler) =>
        Handle(new EventTransformationWrapper<TEvent>(handler));

    public EventHandlersBuilder Transform<TEvent, TTransformedEvent>(
        IEventTransformation<TEvent, TTransformedEvent> handler) =>
        Handle(handler);

    public EventHandlersBuilder Filter<TEvent>(
        Func<TEvent, bool> handler) =>
        Handle(new EventFilterWrapper<TEvent>(handler));

    public EventHandlersBuilder Filter<TEvent>(
        Func<TEvent, CancellationToken, ValueTask<bool>> handler) =>
        Handle(new EventFilterWrapper<TEvent>(handler));

    public EventHandlersBuilder Filter<TEvent>(
        IEventFilter<TEvent> handler) =>
        Handle(handler);

    public IEnumerable<IEventHandler> Build() => eventHandlers;
}
