namespace Core.Events;

public interface IEventHandler<in TEvent>
{
    Task Handle(TEvent @event, CancellationToken ct);
}

public class EventHandler<TEvent> : IEventHandler<TEvent>
{
    private readonly Func<TEvent,CancellationToken,Task> handler;

    public EventHandler(Func<TEvent, CancellationToken, Task> handler) =>
        this.handler = handler;

    public Task Handle(TEvent @event, CancellationToken ct) =>
        handler(@event, ct);
}
