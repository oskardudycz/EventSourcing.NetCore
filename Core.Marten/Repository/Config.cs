using Core.Aggregates;
using Core.OpenTelemetry;
using Core.OptimisticConcurrency;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.Marten.Repository;

public static class Config
{
    public static IServiceCollection AddMartenRepository<T>(
        this IServiceCollection services,
        bool withAppendScope = true,
        bool withTelemetry = true
    ) where T : class, IAggregate
    {
        services.AddScoped<IMartenRepository<T>, MartenRepository<T>>();

        if (withAppendScope)
            services.Decorate<IMartenRepository<T>>(
                (inner, sp) => new MartenRepositoryWithETagDecorator<T>(
                    inner,
                    sp.GetRequiredService<IExpectedResourceVersionProvider>(),
                    sp.GetRequiredService<INextResourceVersionProvider>()
                )
            );

        if (withTelemetry)
        {
            services.Decorate<IMartenRepository<T>>(
                (inner, sp) => new MartenRepositoryWithTracingDecorator<T>(
                    inner,
                    sp.GetRequiredService<IDocumentSession>(),
                    sp.GetRequiredService<IActivityScope>(),
                    sp.GetRequiredService<ILogger<MartenRepositoryWithTracingDecorator<T>>>()
                )
            );
        }

        return services;
    }
}
