namespace Core.Tracing.Correlation;

public record CorrelationId(string Value)
{
    public const string LoggerScopeKey = "Correlation-ID";
}

public interface ICorrelationIdFactory
{
    CorrelationId New();
}


public class GuidCorrelationIdFactory: ICorrelationIdFactory
{
    public CorrelationId New() => new(Guid.NewGuid().ToString("N"));
}

public interface ICorrelationIdProvider
{
    void Set(CorrelationId correlationId);
    CorrelationId? Get();
}

public class CorrelationIdProvider: ICorrelationIdProvider
{
    private CorrelationId? value;

    public void Set(CorrelationId correlationId) => value = correlationId;

    public CorrelationId? Get() => value;
}
