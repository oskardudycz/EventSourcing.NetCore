using ECommerce.Domain.Core.Entities;

namespace ECommerce.Domain.Core.Repositories;

public interface IReadonlyRepository<TEntity> where TEntity : class, IEntity
{
    Task<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    IQueryable<TEntity> Query();
}
