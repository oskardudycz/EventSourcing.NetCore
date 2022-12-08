using Core.Aggregates;
using Core.OptimisticConcurrency;
using Microsoft.Extensions.DependencyInjection;

namespace Core.EventStoreDB.Repository;

public static class Config
{
    public static IServiceCollection AddEventStoreDBRepository<T>(
        this IServiceCollection services,
        bool withAppendScope = true
    ) where T : class, IAggregate
    {
        services.AddScoped<EventStoreDBRepository<T>, EventStoreDBRepository<T>>();

        if (!withAppendScope)
        {
            services.AddScoped<IEventStoreDBRepository<T>, EventStoreDBRepository<T>>();
        }
        else
        {
            services.AddScoped<IEventStoreDBRepository<T>, EventStoreDBRepositoryWithETagDecorator<T>>(
                sp => new EventStoreDBRepositoryWithETagDecorator<T>(
                    sp.GetRequiredService<EventStoreDBRepository<T>>(),
                    sp.GetRequiredService<IExpectedResourceVersionProvider>(),
                    sp.GetRequiredService<INextResourceVersionProvider>()
                )
            );
        }

        return services;
    }
}

