namespace Core.Events;

public interface IEventHandler<in TEvent>
{
    Task Handle(TEvent @event, CancellationToken ct);
}

public class EventHandler<TEvent>(Func<TEvent, CancellationToken, Task> handler): IEventHandler<TEvent>
{
    public Task Handle(TEvent @event, CancellationToken ct) =>
        handler(@event, ct);
}
