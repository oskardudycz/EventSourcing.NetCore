using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core.EntityFramework.Queries;

public static class QueryHandler
{

    public static IServiceCollection AddEntityFrameworkQueryHandler<TDbContext, TQuery, TResult>(
        this IServiceCollection services,
        Func<IQueryable<TResult>, TQuery, CancellationToken, Task<TResult>> handler
    )
        where TDbContext : DbContext
        where TResult : class
        => services.AddEntityFrameworkQueryHandler<TDbContext, TResult, TQuery, TResult>(handler);

    public static IServiceCollection AddEntityFrameworkQueryHandler<TDbContext, TView, TQuery, TResult>(
        this IServiceCollection services,
        Func<IQueryable<TView>, TQuery, CancellationToken, Task<TResult>> handler
    )
        where TDbContext : DbContext
        where TView : class
        =>
            services.AddQueryHandler<TQuery, TResult>(sp =>
            {
                var queryable =
                    sp.GetRequiredService<TDbContext>()
                        .Set<TView>()
                        .AsNoTracking()
                        .AsQueryable();

                return (query, ct) =>
                    handler(queryable, query, ct);
            });

    public static IServiceCollection AddEntityFrameworkQueryHandler<TDBContext, TQuery, TResult>(
        this IServiceCollection services,
        Func<IQueryable<TResult>, TQuery, CancellationToken, Task<IReadOnlyList<TResult>>> handler
    )
        where TDBContext : DbContext where TResult : class
        =>
            services.AddQueryHandler<TQuery, IReadOnlyList<TResult>>(sp =>
            {
                var queryable =
                    sp.GetRequiredService<TDBContext>()
                        .Set<TResult>()
                        .AsNoTracking()
                        .AsQueryable();

                return (query, ct) =>
                    handler(queryable, query, ct);
            });

    public static IServiceCollection AddQueryHandler<TQuery, TResult>(
        this IServiceCollection services,
        Func<IServiceProvider, Func<TQuery, CancellationToken, Task<TResult>>> setup
    ) =>
        services.AddTransient(setup);
}
