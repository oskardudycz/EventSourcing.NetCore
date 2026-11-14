using Core.Aggregates;
using Core.OpenTelemetry;
using Core.OptimisticConcurrency;
using Microsoft.Extensions.DependencyInjection;

namespace Core.KurrentDB.Repository;

public static class Config
{
    public static IServiceCollection AddKurrentDBRepository<T>(
        this IServiceCollection services,
        bool withAppendScope = true,
        bool withTelemetry = true
    ) where T : class, IAggregate
    {
        services.AddScoped<IKurrentDBRepository<T>, KurrentDBRepository<T>>();

        if (withAppendScope)
        {
            services.Decorate<IKurrentDBRepository<T>>(
                (inner, sp) => new KurrentDBRepositoryWithETagDecorator<T>(
                    inner,
                    sp.GetRequiredService<IExpectedResourceVersionProvider>(),
                    sp.GetRequiredService<INextResourceVersionProvider>()
                )
            );
        }

        if (withTelemetry)
        {
            services.Decorate<IKurrentDBRepository<T>>(
                (inner, sp) => new KurrentDBRepositoryWithTelemetryDecorator<T>(
                    inner,
                    sp.GetRequiredService<IActivityScope>()
                )
            );
        }

        return services;
    }
}

