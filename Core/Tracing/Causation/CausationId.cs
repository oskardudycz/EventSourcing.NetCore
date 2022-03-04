using Core.Tracing.Correlation;

namespace Core.Tracing.Causation;

public record CausationId(string Value);

public interface ICausationIdIdFactory
{
    CausationId New();
}


public class GuidCausationIdFactory: ICorrelationIdFactory
{
    public CorrelationId New() => new(Guid.NewGuid().ToString("N"));
}
