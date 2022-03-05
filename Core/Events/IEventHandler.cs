namespace Core.Events;

public interface IEventHandler<in TEvent>
{
    Task Handle(TEvent @event, CancellationToken ct);
}
