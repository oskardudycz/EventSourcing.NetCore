namespace Core.Tracing.Causation;

public record CausationId(string Value)
{
    public const string LoggerScopeKey = "Causation-ID";
}

public interface ICausationIdFactory
{
    CausationId New();
}


public class GuidCausationIdFactory: ICausationIdFactory
{
    public CausationId New() => new(Guid.NewGuid().ToString("N"));
}

public interface ICausationIdProvider
{
    void Set(CausationId causationId);
    CausationId? Get();
}

public class CausationIdProvider: ICausationIdProvider
{
    private CausationId? value;

    public void Set(CausationId causationId) => value = causationId;

    public CausationId? Get() => value;
}
