namespace ECommerce.Core.Repositories;

public interface IRepository<TEntity> where TEntity : class, IEntity
{
    TEntity Add(TEntity entity);
    TEntity Update(TEntity entity);
    TEntity Delete(TEntity entity);
    ValueTask<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken ct);
}
