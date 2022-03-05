using Core.Marten.Events;
using Core.Tracing;

namespace Carts.Tests.Stubs.Events;

public class DummyMartenAppendScope: IMartenAppendScope
{
    private readonly long? expectedVersion;
    private readonly TraceMetadata? traceMetadata;

    public DummyMartenAppendScope(long? expectedVersion = null, TraceMetadata? traceMetadata = null)
    {
        this.expectedVersion = expectedVersion;
        this.traceMetadata = traceMetadata;
    }

    public async Task Do(Func<long?, TraceMetadata?, Task<long>> handler)
    {
        await handler(expectedVersion, traceMetadata);
    }
}
