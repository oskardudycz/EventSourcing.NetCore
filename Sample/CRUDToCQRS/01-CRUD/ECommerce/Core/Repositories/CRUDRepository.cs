using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Core.Repositories;

public class CRUDRepository<TEntity>: ICRUDRepository<TEntity>
    where TEntity : class, IEntity
{
    private readonly DbContext dbContext;

    protected CRUDRepository(DbContext dbContext)
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

    public ValueTask<TEntity?> FindByIdAsync(Guid id, CancellationToken ct)
    {
        return dbContext.Set<TEntity>().FindAsync([id], ct);
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
