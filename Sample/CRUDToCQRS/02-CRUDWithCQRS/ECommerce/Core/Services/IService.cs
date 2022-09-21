using ECommerce.Core.Requests;

namespace ECommerce.Core.Services;

public interface IService
{
    Task CreateAsync<TCreateRequest, TCreateResponse>(TCreateRequest request, CancellationToken ct);
    Task UpdateAsync<TUpdateRequest, TUpdateResponse>(TUpdateRequest request, CancellationToken ct)
        where TUpdateRequest: IUpdateRequest;
    Task DeleteByIdAsync(Guid id, CancellationToken ct);
}
