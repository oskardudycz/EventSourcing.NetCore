using ECommerce.Core.Requests;

namespace ECommerce.Core.Services;

public interface ICRUDService
{
    Task<TCreateResponse> CreateAsync<TCreateRequest, TCreateResponse>(TCreateRequest request, CancellationToken ct);
    Task<TUpdateResponse> UpdateAsync<TUpdateRequest, TUpdateResponse>(TUpdateRequest request, CancellationToken ct)
        where TUpdateRequest: IUpdateRequest;
    Task<bool> DeleteByIdAsync(Guid id, CancellationToken ct);
    Task<TResponse> GetByIdAsync<TResponse>(Guid id, CancellationToken ct);
    Task<List<TResponse>> GetPagedAsync<TResponse>(CancellationToken ct, int pageNumber = 1, int pageSize = 20);
}
