using Core.EventStoreDB.OptimisticConcurrency;

namespace Carts.Tests.Stubs.Events;

public class DummyEventStoreDBAppendScope: IEventStoreDBAppendScope
{
    private readonly ulong? expectedVersion;

    public DummyEventStoreDBAppendScope(ulong? expectedVersion = null)
    {
        this.expectedVersion = expectedVersion;
    }

    public async Task Do(Func<ulong?, Task<ulong>> handler)
    {
        await handler(expectedVersion);
    }
}
