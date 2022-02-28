namespace Core.WebApi.OptimisticConcurrency;

public record ResourceVersion(string Value);

public class ExpectedResourceVersionProvider
{
    private ResourceVersion? expectedVersion;

    public void Set(ResourceVersion version) =>
        expectedVersion = version;

    public ResourceVersion? Get() => expectedVersion;
}

public class NextResourceVersionProvider
{
    private ResourceVersion? nextResourceVersion;

    public void Set(ResourceVersion version) =>
        nextResourceVersion = version;

    public ResourceVersion? Get() => nextResourceVersion;
}
