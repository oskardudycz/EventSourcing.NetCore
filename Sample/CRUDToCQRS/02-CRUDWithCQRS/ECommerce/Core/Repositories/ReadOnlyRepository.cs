using Microsoft.EntityFrameworkCore;

namespace ECommerce.Core.Repositories;

public abstract class ReadOnlyRepository<TEntity>(IQueryable<TEntity> query): IReadonlyRepository<TEntity>
    where TEntity : class, IEntity
{
    public Task<TEntity?> FindByIdAsync(Guid id, CancellationToken ct) =>
        query.SingleOrDefaultAsync(e => e.Id == id, ct);

    public IQueryable<TEntity> Query() => query;
}
