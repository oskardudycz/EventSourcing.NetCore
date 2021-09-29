using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Core.Events;
using ECommerce.Core.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core.Projections
{
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

    public class EntityFrameworkProjectionBuilder<TView, TDbContext>
        where TView : class
        where TDbContext : DbContext
    {
        public readonly IServiceCollection services;

        public EntityFrameworkProjectionBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        public EntityFrameworkProjectionBuilder<TView, TDbContext> AddOn<TEvent>(Func<TEvent, TView> handler)
        {
            services.AddSingleton(handler);
            services.AddTransient<IEventHandler<TEvent>, AddProjection<TView, TEvent, TDbContext>>();

            return this;
        }

        public EntityFrameworkProjectionBuilder<TView, TDbContext> UpdateOn<TEvent>(
            Func<TEvent, object> getViewId,
            Action<TEvent, TView> handler)
        {
            services.AddSingleton(getViewId);
            services.AddSingleton(handler);
            services.AddTransient<IEventHandler<TEvent>, UpdateProjection<TView, TEvent, TDbContext>>();

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

    public class AddProjection<TView, TEvent, TDbContext>: IEventHandler<TEvent>
        where TView: class
        where TDbContext: DbContext
    {
        private readonly TDbContext dbContext;
        private readonly Func<TEvent, TView> create;

        public AddProjection(
            TDbContext dbContext,
            Func<TEvent, TView> create
        )
        {
            this.dbContext = dbContext;
            this.create = create;
        }

        public async Task Handle(TEvent @event, CancellationToken ct)
        {
            var view = create(@event);

            await dbContext.AddAsync(view, ct);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    public class UpdateProjection<TView, TEvent, TDbContext>: IEventHandler<TEvent>
        where TView: class
        where TDbContext: DbContext
    {
        private readonly TDbContext dbContext;
        private readonly Func<TEvent, object> getViewId;
        private readonly Action<TEvent, TView> update;

        public UpdateProjection(
            TDbContext dbContext,
            Func<TEvent, object> getViewId,
            Action<TEvent, TView> update
        )
        {
            this.dbContext = dbContext;
            this.getViewId = getViewId;
            this.update = update;
        }

        public async Task Handle(TEvent @event, CancellationToken ct)
        {
            var viewId = getViewId(@event);
            var view = await dbContext.FindAsync<TView>(new [] {viewId}, ct);

            update(@event, view);

            await dbContext.SaveChangesAsync(ct);
        }
    }
}
