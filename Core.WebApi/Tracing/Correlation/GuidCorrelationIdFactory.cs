namespace Core.WebApi.Tracing.Correlation;

public record CorrelationId(string Value);

public interface ICorrelationIdFactory
{
    CorrelationId New();
}


public class GuidCorrelationIdFactory: ICorrelationIdFactory
{
    public CorrelationId New() => new(Guid.NewGuid().ToString("N"));
}
