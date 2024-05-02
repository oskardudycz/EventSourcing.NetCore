using Core.Events;
using ECommerce.Core.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core.Projections;

public static class EntityFrameworkProjection
{
    public static IServiceCollection For<TView, TDbContext>(
        this IServiceCollection services,
        Action<EntityFrameworkProjectionBuilder<TView, TDbContext>> setup
    )
        where TView: class
        where TDbContext: DbContext
    {
        setup(new EntityFrameworkProjectionBuilder<TView, TDbContext>(services));
        return services;
    }
}

public class EntityFrameworkProjectionBuilder<TView, TDbContext>(IServiceCollection services)
    where TView : class
    where TDbContext : DbContext
{
    public readonly IServiceCollection services = services;

    public EntityFrameworkProjectionBuilder<TView, TDbContext> AddOn<TEvent>(Func<EventEnvelope<TEvent>, TView> handler) where TEvent : notnull
    {
        services.AddSingleton(handler);
        services.AddTransient<IEventHandler<EventEnvelope<TEvent>>, AddProjection<TView, TEvent, TDbContext>>();

        return this;
    }

    public EntityFrameworkProjectionBuilder<TView, TDbContext> UpdateOn<TEvent>(
        Func<TEvent, object> getViewId,
        Action<EventEnvelope<TEvent>, TView> handler,
        Func<EntityEntry<TView>, CancellationToken, Task>? prepare = null
    ) where TEvent : notnull
    {
        services.AddSingleton(getViewId);
        services.AddSingleton(handler);
        services.AddTransient<IEventHandler<EventEnvelope<TEvent>>, UpdateProjection<TView, TEvent, TDbContext>>();

        if (prepare != null)
        {
            services.AddSingleton(prepare);
        }

        return this;
    }

    public EntityFrameworkProjectionBuilder<TView, TDbContext> QueryWith<TQuery>(
        Func<IQueryable<TView>, TQuery, CancellationToken, Task<TView>> handler
    )
    {
        services.AddEntityFrameworkQueryHandler<TDbContext, TQuery, TView>(handler);

        return this;
    }

    public EntityFrameworkProjectionBuilder<TView, TDbContext> QueryWith<TQuery>(
        Func<IQueryable<TView>, TQuery, CancellationToken, Task<IReadOnlyList<TView>>> handler
    )
    {
        services.AddEntityFrameworkQueryHandler<TDbContext, TQuery, TView>(handler);

        return this;
    }
}

public class AddProjection<TView, TEvent, TDbContext>(
    TDbContext dbContext,
    Func<EventEnvelope<TEvent>, TView> create)
    : IEventHandler<EventEnvelope<TEvent>>
    where TView : class
    where TDbContext : DbContext
    where TEvent : notnull
{
    public async Task Handle(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct)
    {
        var view = create(eventEnvelope);

        await dbContext.AddAsync(view, ct);
        await dbContext.SaveChangesAsync(ct);
    }
}

public class UpdateProjection<TView, TEvent, TDbContext>(
    TDbContext dbContext,
    Func<TEvent, object> getViewId,
    Action<EventEnvelope<TEvent>, TView> update,
    Func<EntityEntry<TView>, CancellationToken, Task>? prepare = null)
    : IEventHandler<EventEnvelope<TEvent>>
    where TView : class
    where TDbContext : DbContext
    where TEvent : notnull
{
    public async Task Handle(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct)
    {
        var viewId = getViewId(eventEnvelope.Data);
        var view = await dbContext.FindAsync<TView>([viewId], ct);

        if (view == null)
            throw new InvalidOperationException($"{typeof(TView).Name} with id {viewId} wasn't found");

        prepare?.Invoke(dbContext.Entry(view), ct);

        update(eventEnvelope, view);

        await dbContext.SaveChangesAsync(ct);
    }
}
