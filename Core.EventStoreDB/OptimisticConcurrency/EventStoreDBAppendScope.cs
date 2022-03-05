using Core.Events;
using Core.Tracing;
using Microsoft.Extensions.DependencyInjection;

namespace Core.EventStoreDB.OptimisticConcurrency;

public interface IEventStoreDBAppendScope: IAppendScope<ulong>
{
}

public class EventStoreDBAppendScope: AppendScope<ulong>, IEventStoreDBAppendScope
{
    public EventStoreDBAppendScope(
        Func<ulong?> getExpectedVersion,
        Action<ulong> setNextExpectedVersion,
        Func<TraceMetadata?> getEventMetadata
    ): base(getExpectedVersion, setNextExpectedVersion, getEventMetadata)
    {
    }
}

public static class EventStoreDBAppendScopeExtensions
{
    public static IServiceCollection AddEventStoreDBAppendScope(this IServiceCollection services) =>
        services
            .AddScoped<EventStoreDBExpectedStreamRevisionProvider, EventStoreDBExpectedStreamRevisionProvider>()
            .AddScoped<EventStoreDBNextStreamRevisionProvider, EventStoreDBNextStreamRevisionProvider>()
            .AddScoped<IEventStoreDBAppendScope, EventStoreDBAppendScope>(
                sp =>
                {
                    var expectedStreamVersionProvider =
                        sp.GetRequiredService<EventStoreDBExpectedStreamRevisionProvider>();
                    var nextStreamVersionProvider = sp.GetRequiredService<EventStoreDBNextStreamRevisionProvider>();
                    var traceMetadataProvider = sp.GetRequiredService<ITraceMetadataProvider>();

                    return new EventStoreDBAppendScope(
                        () => expectedStreamVersionProvider.Value,
                        nextStreamVersionProvider.Set,
                        traceMetadataProvider.Get
                    );
                });
}
