using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Core.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core.Projections
{
    public abstract class EntityFrameworkProjection<T>
        where T: class
    {
        private readonly DbContext dbContext;

        protected EntityFrameworkProjection(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected async Task Add(T entity, CancellationToken ct)
        {
            await dbContext.AddAsync(entity, ct);
            await dbContext.SaveChangesAsync(ct);
        }

        protected async Task Update(Guid id, Action<T> update, CancellationToken ct)
        {
            var entity = await dbContext.FindAsync<T>(new object[] {id}, ct);

            update(entity);

            await dbContext.SaveChangesAsync(ct);
        }
    }

    public static class EntityFrameworkProjectionConfiguration
    {
        public static IServiceCollection AddProjection<TProjection>(this IServiceCollection services, params Type[] eventTypes)
            where TProjection : class
        {
            services.AddTransient<TProjection, TProjection>();
            Type generic = typeof(IEventHandler<>);

            foreach (var eventType in eventTypes)
            {
                services.AddTransient(
                    generic.MakeGenericType(eventType),
                    sp => sp.GetRequiredService<TProjection>()
                );
            }

            return services;
        }
    }
}
