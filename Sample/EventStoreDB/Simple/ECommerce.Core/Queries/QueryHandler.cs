using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core.Queries;

public static class QueryHandler
{
    public static IServiceCollection AddEntityFrameworkQueryHandler<TDbContext, TQuery, TResult>(
        this IServiceCollection services,
        Func<IQueryable<TResult>, TQuery, CancellationToken, Task<TResult>> handler
    )
        where TDbContext : DbContext where TResult : class
        =>
            services.AddQueryHandler<TQuery, TResult>(sp =>
            {
                var queryable =
                    sp.GetRequiredService<TDbContext>()
                        .Set<TResult>()
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