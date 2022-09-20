using Microsoft.EntityFrameworkCore;

namespace ECommerce.Core.Repositories;

public class CRUDRepository<TDbContext, TEntity>: ICRUDRepository<TEntity> where TDbContext : DbContext
    where TEntity : class, IEntity
{
    private readonly TDbContext dbContext;

    public CRUDRepository(TDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public TEntity Add(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entry = dbContext.Add(entity);

        return entry.Entity;
    }

    public TEntity Update(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entry = dbContext.Update(entity);

        return entry.Entity;
    }

    public TEntity Delete(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entry = dbContext.Remove(entity);

        return entry.Entity;
    }

    public ValueTask<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<TEntity>().FindAsync(
            new object?[] { id },
            cancellationToken: cancellationToken
        );
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return dbContext.SaveChangesAsync(ct);
    }

    public IQueryable<TEntity> Query()
    {
        return dbContext.Set<TEntity>();
    }
}
