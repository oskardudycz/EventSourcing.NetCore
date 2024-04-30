using Microsoft.EntityFrameworkCore;

namespace ECommerce.Core.Repositories;

public abstract class Repository<TEntity>: IRepository<TEntity>
    where TEntity : class, IEntity
{
    private readonly DbContext dbContext;

    protected Repository(DbContext dbContext)
    {
        this.dbContext = dbContext;
    }

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
