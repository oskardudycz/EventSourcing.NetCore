using ECommerce.Domain.Core.Requests;

namespace ECommerce.Domain.Core.Services;

public interface IService
{
    Task CreateAsync<TCreateRequest>(TCreateRequest request, CancellationToken ct);
    Task UpdateAsync<TUpdateRequest>(TUpdateRequest request, CancellationToken ct)
        where TUpdateRequest: IUpdateRequest;
    Task DeleteByIdAsync(Guid id, CancellationToken ct);
}
