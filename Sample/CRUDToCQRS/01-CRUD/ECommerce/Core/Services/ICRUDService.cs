namespace ECommerce.Core.Services;

public interface ICRUDService<TEntity> where TEntity : class, IEntity, new()
{
    Task<TCreateResponse> CreateAsync<TCreateRequest, TCreateResponse>(TCreateRequest request, CancellationToken ct);
    Task<TUpdateResponse> UpdateAsync<TUpdateRequest, TUpdateResponse>(TUpdateRequest request, CancellationToken ct);
    Task<bool> DeleteByIdAsync(Guid id, CancellationToken ct);
    Task<TResponse> GetByIdAsync<TResponse>(Guid id, CancellationToken ct);
    Task<List<TResponse>> GetPagedAsync<TResponse>(CancellationToken ct, int pageNumber = 1, int pageSize = 20);
    IQueryable<TEntity> Query();
}
