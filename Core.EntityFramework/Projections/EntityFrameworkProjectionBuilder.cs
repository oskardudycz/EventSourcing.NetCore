using System.Data;
using System.Linq.Expressions;
using Core.EntityFramework.Queries;
using Core.EntityFramework.Subscriptions.Checkpoints;
using Core.Events;
using Core.EventStoreDB;
using Core.EventStoreDB.Subscriptions;
using Core.EventStoreDB.Subscriptions.Checkpoints;
using Core.EventStoreDB.Subscriptions.Checkpoints.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Core.EntityFramework.Projections;

public static class EntityFrameworkProjection
{
    public static IServiceCollection AddEntityFrameworkProject<TDbContext>(
        this IServiceCollection services
    ) where TDbContext : DbContext =>
        services.AddPostgresCheckpointing()
            .AddSingleton<PostgresSubscriptionCheckpointSetup>()
            .AddScoped<NpgsqlTransaction>(sp =>
            {
                var dbContext = sp.GetRequiredService<TDbContext>();
                return (NpgsqlTransaction)dbContext.Database.BeginTransaction().GetDbTransaction();
            })
            .AddScoped<PostgresConnectionProvider>(sp =>
                PostgresConnectionProvider.From(sp.GetRequiredService<NpgsqlTransaction>())
            )
            .AddScoped<ISubscriptionCheckpointRepository>(sp =>
            {
                var dbContext = sp.GetRequiredService<TDbContext>();

                var checkpointTransaction = new EFCheckpointTransaction(dbContext);

                var connectionProvider = sp.GetRequiredService<PostgresConnectionProvider>();

                return new TransactionalPostgresSubscriptionCheckpointRepository(
                    new PostgresSubscriptionCheckpointRepository(connectionProvider),
                    checkpointTransaction
                );
            });

    public static IServiceCollection For<TView, TId, TDbContext>(
        this IServiceCollection services,
        Action<EntityFrameworkProjectionBuilder<TView, TId, TDbContext>> setup
    )
        where TView : class
        where TDbContext : DbContext
        where TId : struct
    {
        var builder = new EntityFrameworkProjectionBuilder<TView, TId, TDbContext>(services);
        setup(builder);

        services.AddEventStoreDBSubscriptionToAll(
            new EventStoreDBSubscriptionToAllOptions { SubscriptionId = typeof(TView).FullName!, BatchSize = 20 },
            false
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

    public EntityFrameworkProjectionBuilder<TView, TId, TDbContext> QueryWith<TQuery>(
        Func<IQueryable<TView>, TQuery, CancellationToken, Task<IReadOnlyList<TView>>> handler
    )
    {
        services.AddEntityFrameworkQueryHandler<TDbContext, TQuery, TView>(handler);

        return this;
    }
}
