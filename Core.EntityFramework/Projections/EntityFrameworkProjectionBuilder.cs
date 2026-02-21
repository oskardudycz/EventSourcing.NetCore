using System.Linq.Expressions;
using Core.EntityFramework.Queries;
using Core.EntityFramework.Subscriptions.Checkpoints;
using Core.Events;
using Core.EventStoreDB;
using Core.EventStoreDB.Subscriptions;
using Core.EventStoreDB.Subscriptions.Batch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core.EntityFramework.Projections;

public static class EntityFrameworkProjection
{
    public static IServiceCollection AddEntityFrameworkProjections<TDbContext>(
        this IServiceCollection services
    ) where TDbContext : DbContext =>
        services.AddScoped<IEventsBatchCheckpointer, TransactionalDbContextEventsBatchCheckpointer<TDbContext>>();

    public static IServiceCollection For<TView, TId, TDbContext>(
        this IServiceCollection services,
        Action<EntityFrameworkProjectionBuilder<TView, TId, TDbContext>> setup,
        int batchSize = 20
    )
        where TView : class
        where TDbContext : DbContext
        where TId : struct
    {
        var builder = new EntityFrameworkProjectionBuilder<TView, TId, TDbContext>(services);
        setup(builder);

        services.AddEventStoreDBSubscriptionToAll(
            new EventStoreDBSubscriptionToAllOptions { SubscriptionId = typeof(TView).FullName!, BatchSize = batchSize },
            sp =>
            {
                var dbContext = sp.GetRequiredService<TDbContext>();
                var projection = builder.Projection;

                projection.DbContext = dbContext;

                return [projection];
            }
        );

        return services;
    }
}

public class EntityFrameworkProjectionBuilder<TView, TId, TDbContext>(IServiceCollection services)
    where TView : class
    where TId : struct
    where TDbContext : DbContext
{
    public EntityFrameworkProjection<TView, TId, TDbContext> Projection = new();

    public EntityFrameworkProjectionBuilder<TView, TId, TDbContext> ViewId(Expression<Func<TView, TId>> id)
    {
        Projection.ViewId(id);
        return this;
    }

    public EntityFrameworkProjectionBuilder<TView, TId, TDbContext> AddOn<TEvent>(
        Func<EventEnvelope<TEvent>, TView> handler)
        where TEvent : notnull
    {
        Projection.Creates(handler);
        return this;
    }

    public EntityFrameworkProjectionBuilder<TView, TId, TDbContext> UpdateOn<TEvent>(
        Func<TEvent, TId> getViewId,
        Action<TView, EventEnvelope<TEvent>> handler
    ) where TEvent : notnull
    {
        Projection.Projects(getViewId, handler);
        return this;
    }

    public EntityFrameworkProjectionBuilder<TView, TId, TDbContext> Include(
        Expression<Func<TView, object>> include)
    {
        Projection.Include = include;

        return this;
    }

    public EntityFrameworkProjectionBuilder<TView, TId, TDbContext> QueryWith<TQuery>(
        Func<IQueryable<TView>, TQuery, CancellationToken, Task<TView>> handler
    )
    {
        services.AddEntityFrameworkQueryHandler<TDbContext, TQuery, TView>(handler);

        return this;
    }

    public EntityFrameworkProjectionBuilder<TView, TId, TDbContext> QueryWith<TQuery, TResult>(
        Func<IQueryable<TView>, TQuery, CancellationToken, Task<TResult>> handler
    )
    {
        services.AddEntityFrameworkQueryHandler<TDbContext, TView, TQuery, TResult>(handler);

        return this;
    }

    public EntityFrameworkProjectionBuilder<TView, TId, TDbContext> QueryWith<TQuery>(
        Func<IQueryable<TView>, TQuery, CancellationToken, Task<IReadOnlyList<TView>>> handler
    )
    {
        services.AddEntityFrameworkQueryHandler<TDbContext, TQuery, TView>(handler);

        return this;
    }
}
