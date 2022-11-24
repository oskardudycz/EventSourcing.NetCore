using Core.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Core.EventStoreDB.OptimisticConcurrency;

public interface IEventStoreDBAppendScope: IAppendScope<ulong>
{
}

public class EventStoreDBAppendScope: AppendScope<ulong>, IEventStoreDBAppendScope
{
    public EventStoreDBAppendScope(
        Func<ulong?> getExpectedVersion,
        Action<ulong> setNextExpectedVersion
    ): base(getExpectedVersion, setNextExpectedVersion)
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

                    return new EventStoreDBAppendScope(
                        () => expectedStreamVersionProvider.Value,
                        nextStreamVersionProvider.Set
                    );
                });
}
