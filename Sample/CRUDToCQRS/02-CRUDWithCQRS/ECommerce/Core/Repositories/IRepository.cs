namespace ECommerce.Core.Repositories;

public interface IRepository<TEntity> where TEntity : class, IEntity
{
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    ValueTask<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken ct);
}
