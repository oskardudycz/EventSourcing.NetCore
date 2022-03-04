﻿using Core.Events;
using Core.Marten.OptimisticConcurrency;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Marten.Events;

public interface IMartenAppendScope: IAppendScope<long>
{
}

public class MartenAppendScope: AppendScope<long>, IMartenAppendScope
{
    public MartenAppendScope(
        Func<long?> getExpectedVersion,
        Action<long> setNextExpectedVersion,
        Func<EventMetadata?> getEventMetadata
    ): base(getExpectedVersion, setNextExpectedVersion, getEventMetadata)
    {
    }
}

public static class MartenAppendScopeExtensions
{
    public static IServiceCollection AddMartenAppendScope(this IServiceCollection services) =>
        services
            .AddScoped<MartenExpectedStreamVersionProvider, MartenExpectedStreamVersionProvider>()
            .AddScoped<MartenNextStreamVersionProvider, MartenNextStreamVersionProvider>()
            .AddScoped<IMartenAppendScope, MartenAppendScope>(
                sp =>
                {
                    var expectedStreamVersionProvider = sp.GetRequiredService<MartenExpectedStreamVersionProvider>();
                    var nextStreamVersionProvider = sp.GetRequiredService<MartenNextStreamVersionProvider>();
                    var eventMetadataProvider = sp.GetRequiredService<IEventMetadataProvider>();

                    return new MartenAppendScope(
                        () => expectedStreamVersionProvider.Value,
                        nextStreamVersionProvider.Set,
                        eventMetadataProvider.Get
                    );
                });
}
