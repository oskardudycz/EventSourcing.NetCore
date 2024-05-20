using Microsoft.EntityFrameworkCore;

namespace Core.EntityFramework;

public static class DbContextExtensions
{
    public static void AddOrUpdate<TEntity>(this DbContext dbContext, TEntity entity) where TEntity : class
    {
        var dbSet = dbContext.Set<TEntity>();
        var entry = dbContext.Entry(entity);

        if (entry.State == EntityState.Detached)
        {
            dbSet.Add(entity);
        }
    }
}
