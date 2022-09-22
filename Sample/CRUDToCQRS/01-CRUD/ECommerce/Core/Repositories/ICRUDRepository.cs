using ECommerce.Core.Entities;

namespace ECommerce.Core.Repositories;

public interface ICRUDRepository<TEntity> where TEntity : class, IEntity
{
    TEntity Add(TEntity entity);
    TEntity Update(TEntity entity);
    TEntity Delete(TEntity entity);
    ValueTask<TEntity?> FindByIdAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    IQueryable<TEntity> Query();
}
