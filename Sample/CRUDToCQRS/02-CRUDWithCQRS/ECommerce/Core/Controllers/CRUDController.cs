using ECommerce.Core.Requests;
using ECommerce.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Core.Controllers;

public abstract class CRUDController<TEntity>: Controller where TEntity : class, IEntity, new()
{
    protected readonly IService Service;
    protected readonly IReadOnlyService<TEntity> ReadOnlyService;
    protected abstract Func<object, string> GetEntityByIdUri { get; }

    protected CRUDController(
        IService service,
        IReadOnlyService<TEntity> readOnlyService
    )
    {
        Service = service;
        ReadOnlyService = readOnlyService;
    }

    protected async Task<IActionResult> CreateAsync<TCreateRequest, TCreateResponse>(
        TCreateRequest request,
        CancellationToken ct
    ) where TCreateRequest : ICreateRequest
    {
        await Service.CreateAsync(request, ct);

        return Created(GetEntityByIdUri(request.Id), request.Id);
    }

    protected async Task<IActionResult> UpdateAsync<TUpdateRequest, TUpdateResponse>(
        TUpdateRequest request,
        CancellationToken ct
    ) where TUpdateRequest: IUpdateRequest
    {
        await Service.UpdateAsync(request, ct);

        Response.Headers.Location = GetEntityByIdUri(request.Id);

        return Ok();
    }

    protected async Task<IActionResult> DeleteByIdAsync(
        Guid id,
        CancellationToken ct
    )
    {
        await Service.DeleteByIdAsync(id, ct);

        return NoContent();
    }
}
