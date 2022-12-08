using Core.Aggregates;
using Core.OpenTelemetry;
using Core.OptimisticConcurrency;
using Microsoft.Extensions.DependencyInjection;

namespace Core.EventStoreDB.Repository;

public static class Config
{
    public static IServiceCollection AddEventStoreDBRepository<T>(
        this IServiceCollection services,
        bool withAppendScope = true,
        bool withTelemetry = true
    ) where T : class, IAggregate
    {
        services.AddScoped<IEventStoreDBRepository<T>, EventStoreDBRepository<T>>();

        if (withAppendScope)
        {
            services.Decorate<IEventStoreDBRepository<T>>(
                (inner, sp) => new EventStoreDBRepositoryWithETagDecorator<T>(
                    inner,
                    sp.GetRequiredService<IExpectedResourceVersionProvider>(),
                    sp.GetRequiredService<INextResourceVersionProvider>()
                )
            );
        }

        if (withTelemetry)
        {
            services.Decorate<IEventStoreDBRepository<T>>(
                (inner, sp) => new EventStoreDBRepositoryWithTelemetryDecorator<T>(
                    inner,
                    sp.GetRequiredService<IActivityScope>()
                )
            );
        }

        return services;
    }
}

