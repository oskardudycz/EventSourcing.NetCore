using ECommerce.Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Domain.Core.Repositories;

public abstract class Repository<TEntity>(DbContext dbContext): IRepository<TEntity>
    where TEntity : class, IEntity
{
    public void Add(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        dbContext.Add(entity);
    }

    public void Update(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        dbContext.Update(entity);
    }

    public void Delete(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        dbContext.Remove(entity);
    }

    public ValueTask<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<TEntity>().FindAsync(
            [id],
            cancellationToken: cancellationToken
        );
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return dbContext.SaveChangesAsync(ct);
    }
}
