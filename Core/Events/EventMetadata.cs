using Core.Tracing.Causation;
using Core.Tracing.Correlation;

namespace Core.Events;

public record EventMetadata(
    CorrelationId? CorrelationId,
    CausationId? CausationId
);

public interface IEventMetadataProvider
{
    EventMetadata? Get();
}

public class EventMetadataProvider: IEventMetadataProvider
{
    private readonly ICorrelationIdProvider correlationIdProvider;
    private readonly ICausationIdProvider causationIdProvider;

    public EventMetadataProvider(
        ICorrelationIdProvider correlationIdProvider,
        ICausationIdProvider causationIdProvider
    )
    {
        this.correlationIdProvider = correlationIdProvider;
        this.causationIdProvider = causationIdProvider;
    }

    public EventMetadata? Get()
    {
        var correlationId = correlationIdProvider.Get();
        var causationId = causationIdProvider.Get();

        if (correlationId == null && causationId == null)
            return null;

        return new EventMetadata(correlationId, causationId);
    }
}
