using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Warehouse.Core.Queries;

public interface IQueryHandler<in T, TResult>
{
    ValueTask<TResult> Handle(T query, CancellationToken ct);
}

public static class QueryHandlerConfiguration
{
    public static IServiceCollection AddQueryHandler<T, TResult, TQueryHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TQueryHandler>? configure = null
    ) where TQueryHandler: class, IQueryHandler<T, TResult>
    {

        if (configure == null)
        {
            services.AddTransient<TQueryHandler, TQueryHandler>();
            services.AddTransient<IQueryHandler<T, TResult>, TQueryHandler>();
        }
        else
        {
            services.AddTransient<TQueryHandler, TQueryHandler>(configure);
            services.AddTransient<IQueryHandler<T, TResult>, TQueryHandler>(configure);
        }

        return services;
    }

    public static IQueryHandler<T, TResult> GetQueryHandler<T, TResult>(this HttpContext context)
        => context.RequestServices.GetRequiredService<IQueryHandler<T, TResult>>();

    public static ValueTask<TResult> SendQuery<T, TResult>(this HttpContext context, T query)
        => context.GetQueryHandler<T, TResult>()
            .Handle(query, context.RequestAborted);
}