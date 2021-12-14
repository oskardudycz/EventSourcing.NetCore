using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Warehouse.Api.Core.Queries;

public interface IQueryHandler<in T, TResult>
{
    ValueTask<TResult> Handle(T query, CancellationToken ct);
}

public delegate ValueTask<TResult> QueryHandler<in T, TResult>(T query, CancellationToken ct);

public static class QueryHandlerConfiguration
{
    public static IServiceCollection AddQueryHandler<T, TResult, TQueryHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TQueryHandler>? configure = null
    ) where TQueryHandler : class, IQueryHandler<T, TResult>
    {
        if (configure == null)
        {
            services
                .AddTransient<TQueryHandler, TQueryHandler>()
                .AddTransient<IQueryHandler<T, TResult>, TQueryHandler>();
        }
        else
        {
            services
                .AddTransient<TQueryHandler, TQueryHandler>(configure)
                .AddTransient<IQueryHandler<T, TResult>, TQueryHandler>(configure);
        }

        services
            .AddTransient<Func<T, CancellationToken, ValueTask<TResult>>>(
                sp => sp.GetRequiredService<IQueryHandler<T, TResult>>().Handle
            )
            .AddTransient<QueryHandler<T, TResult>>(
                sp => sp.GetRequiredService<IQueryHandler<T, TResult>>().Handle
            );

        return services;
    }
}
