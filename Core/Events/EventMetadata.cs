using Core.Tracing.Causation;
using Core.Tracing.Correlation;

namespace Core.Events;

public record EventMetadata(
    CorrelationId? CorrelationId,
    CausationId? CausationId
);
