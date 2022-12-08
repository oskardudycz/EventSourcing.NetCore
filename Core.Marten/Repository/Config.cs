using Core.Aggregates;
using Core.OptimisticConcurrency;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Marten.Repository;

public static class Config
{
    public static IServiceCollection AddMartenRepository<T>(
        this IServiceCollection services,
        bool withAppendScope = true
    ) where T : class, IAggregate
    {
        services.AddScoped<MartenRepository<T>, MartenRepository<T>>();

        if (!withAppendScope)
        {
            services.AddScoped<IMartenRepository<T>, MartenRepository<T>>();
        }
        else
        {
            services.AddScoped<IMartenRepository<T>, MartenRepositoryWithETagDecorator<T>>(
                sp => new MartenRepositoryWithETagDecorator<T>(
                    sp.GetRequiredService<MartenRepository<T>>(),
                    sp.GetRequiredService<IExpectedResourceVersionProvider>(),
                    sp.GetRequiredService<INextResourceVersionProvider>()
                )
            );
        }

        return services;
    }
}
