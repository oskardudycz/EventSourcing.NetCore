using Core.Marten.Events;

namespace Carts.Tests.Stubs.Events;

public class DummyMartenAppendScope: IMartenAppendScope
{
    private readonly long? expectedVersion;

    public DummyMartenAppendScope(long? expectedVersion = null)
    {
        this.expectedVersion = expectedVersion;
    }

    public async Task Do(Func<long?, Task<long>> handler)
    {
        await handler(expectedVersion);
    }
}
