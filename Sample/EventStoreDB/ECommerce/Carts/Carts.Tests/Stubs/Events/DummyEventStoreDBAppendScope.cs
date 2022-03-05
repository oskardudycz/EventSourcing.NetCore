using Core.EventStoreDB.OptimisticConcurrency;
using Core.Tracing;

namespace Carts.Tests.Stubs.Events;

public class DummyEventStoreDBAppendScope: IEventStoreDBAppendScope
{
    private readonly ulong? expectedVersion;
    private readonly TraceMetadata? traceMetadata;

    public DummyEventStoreDBAppendScope(ulong? expectedVersion = null, TraceMetadata? traceMetadata = null)
    {
        this.expectedVersion = expectedVersion;
        this.traceMetadata = traceMetadata;
    }

    public async Task Do(Func<ulong?, TraceMetadata?, Task<ulong>> handler)
    {
        await handler(expectedVersion, traceMetadata);
    }
}
