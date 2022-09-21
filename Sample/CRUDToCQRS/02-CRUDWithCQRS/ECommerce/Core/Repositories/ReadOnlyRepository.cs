using Microsoft.EntityFrameworkCore;

namespace ECommerce.Core.Repositories;

public abstract class ReadOnlyRepository<TEntity>: IReadonlyRepository<TEntity>
    where TEntity : class, IEntity
{
    private readonly IQueryable<TEntity> query;

    protected ReadOnlyRepository(IQueryable<TEntity> query)
    {
        this.query = query;
    }

    public Task<TEntity?> FindByIdAsync(Guid id, CancellationToken ct)
    {
        return query.SingleOrDefaultAsync(e => e.Id == id, ct);
    }

    public IQueryable<TEntity> Query()
    {
        return query;
    }
}
