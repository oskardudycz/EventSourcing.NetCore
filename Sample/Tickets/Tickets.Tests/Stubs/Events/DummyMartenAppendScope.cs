using Core.Events;
using Core.Marten.Events;

namespace Carts.Tests.Stubs.Events;

public class DummyMartenAppendScope: IMartenAppendScope
{
    private readonly long? expectedVersion;
    private readonly EventMetadata? eventMetadata;

    public DummyMartenAppendScope(long? expectedVersion = null, EventMetadata? eventMetadata = null)
    {
        this.expectedVersion = expectedVersion;
        this.eventMetadata = eventMetadata;
    }

    public async Task Do(Func<long?, EventMetadata?, Task<long>> handler)
    {
        await handler(expectedVersion, eventMetadata);
    }
}
