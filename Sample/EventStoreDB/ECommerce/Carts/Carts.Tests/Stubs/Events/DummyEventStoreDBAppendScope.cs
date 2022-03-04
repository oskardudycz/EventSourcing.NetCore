using Core.Events;
using Core.EventStoreDB.OptimisticConcurrency;

namespace Carts.Tests.Stubs.Events;

public class DummyEventStoreDBAppendScope: IEventStoreDBAppendScope
{
    private readonly ulong? expectedVersion;
    private readonly EventMetadata? eventMetadata;

    public DummyEventStoreDBAppendScope(ulong? expectedVersion = null, EventMetadata? eventMetadata = null)
    {
        this.expectedVersion = expectedVersion;
        this.eventMetadata = eventMetadata;
    }

    public async Task Do(Func<ulong?, EventMetadata?, Task<ulong>> handler)
    {
        await handler(expectedVersion, eventMetadata);
    }
}
