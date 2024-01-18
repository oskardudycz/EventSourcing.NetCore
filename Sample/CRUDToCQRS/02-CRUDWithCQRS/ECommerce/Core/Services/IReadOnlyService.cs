namespace ECommerce.Core.Services;

public interface IReadOnlyService<TEntity> where TEntity : class, IEntity, new()
{
    Task<TResponse> GetByIdAsync<TResponse>(Guid id, CancellationToken ct);
    Task<List<TResponse>> GetPagedAsync<TResponse>(CancellationToken ct, int pageNumber = 1, int pageSize = 20);
    IQueryable<TEntity> Query();
}
