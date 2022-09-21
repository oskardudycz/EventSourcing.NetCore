using ECommerce.Core.Requests;
using ECommerce.Core.Responses;
using ECommerce.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Core.Controllers;

public abstract class CRUDController: Controller
{
    protected readonly ICRUDService service;

    protected CRUDController(ICRUDService service)
    {
        this.service = service;
    }

    protected async Task<IActionResult> CreateAsync<TCreateRequest, TCreateResponse>(
        TCreateRequest request,
        CancellationToken ct
    ) where TCreateResponse : ICreatedResponse
    {
        var result = await service.CreateAsync<TCreateRequest, TCreateResponse>(request, ct);

        return Ok(result);
    }

    protected async Task<IActionResult> UpdateAsync<TUpdateRequest, TUpdateResponse>(
        TUpdateRequest request,
        CancellationToken ct
    ) where TUpdateRequest: IUpdateRequest
    {
        var result = await service.UpdateAsync<TUpdateRequest, TUpdateResponse>(request, ct);

        return Ok(result);
    }

    protected async Task<IActionResult> DeleteByIdAsync(
        Guid id,
        CancellationToken ct
    )
    {
        await service.DeleteByIdAsync(id, ct);

        return Ok();
    }
}
