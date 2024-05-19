namespace Core.Events;

public interface IEventBatchHandler
{
    Task Handle(IEventEnvelope[] events, CancellationToken ct);
}
