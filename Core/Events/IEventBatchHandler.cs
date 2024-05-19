namespace Core.Events;

public interface IEventBatchHandler
{
    Task Handle(IEventEnvelope[] eventInEnvelopes, CancellationToken ct);
}
